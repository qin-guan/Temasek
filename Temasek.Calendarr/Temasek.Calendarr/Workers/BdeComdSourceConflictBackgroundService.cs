using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using MemoryPack;
using Microsoft.Extensions.Options;
using Polly;
using Temasek.Calendarr.Extensions;
using Temasek.Calendarr.Models;
using Temasek.Calendarr.Options;
using Temasek.Calendarr.Services;

namespace Temasek.Calendarr.Workers;

public partial class BdeComdSourceConflictBackgroundService(
    [FromKeyedServices("BdeComd")] CalendarService calendarService,
    [FromKeyedServices("BackgroundService")]
    ResiliencePipeline pipeline,
    ILogger<BdeComdSourceConflictBackgroundService> logger,
    IOptions<BdeComdOptions> options
) : BackgroundService
{
    private string? _syncToken;

    [GeneratedRegex(@"\[.*((?i)((bc)|(all))).*\]")]
    private static partial Regex BcSummaryRegex();

    [GeneratedRegex(@"(.*|!*)(\[|\<|\().*(\]|\>|\))(\s*-*\s*)*")]
    private static partial Regex SummaryNormalizationRegex();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting worker");

        while (!stoppingToken.IsCancellationRequested)
        {
            await pipeline.ExecuteAsync(async ct =>
            {
                // if (_syncToken is null)
                // {
                //     var tokenReq = calendarService.Events.List(options.Value.SourceCalendarId);
                //
                //     var tokenRes = await tokenReq.ExecuteAsync(ct);
                //     _syncToken = tokenRes.NextSyncToken;
                //
                //     logger.LogInformation("Running sync with token : {SyncToken}", _syncToken);
                // }
                //
                var changedEvents = await calendarService.Events.ListAllAsync(
                    options.Value.SourceCalendarId,
                    // syncToken: _syncToken,
                    ct: ct
                );

                if (changedEvents.Count == 0)
                {
                    logger.LogInformation("No changes since last update");
                    return;
                }

                logger.LogInformation("Syncing changes since last update : {Diff}", changedEvents.Count);

                var targetEvents = await calendarService.Events.ListAllAsync(options.Value.TargetCalendarId, ct: ct);
                var defaultMeta = new BdeComdSourceCalendarEventMetadataV1
                {
                    LastBdeComdEventSync = null
                };

                var eventsToCheck = changedEvents
                    .Where(e => BcSummaryRegex().IsMatch(e.Summary))
                    .Select(e =>
                    {
                        e.ExtendedProperties ??= new Event.ExtendedPropertiesData();
                        var v = e.ExtendedProperties.Shared?[BdeComdSourceCalendarEventMetadata.Key];
                        if (v is null)
                        {
                            e.ExtendedProperties.Shared ??= new Dictionary<string, string>();

                            return new
                            {
                                Metadata = defaultMeta,
                                Event = e,
                            };
                        }

                        var byteA = Convert.FromBase64String(v);

                        return new
                        {
                            Metadata = MemoryPackSerializer.Deserialize<BdeComdSourceCalendarEventMetadataV1>(byteA) ??
                                       defaultMeta,
                            Event = e
                        };
                    })
                    .Where(d => d.Metadata.LastBdeComdEventSync is null)
                    .ToImmutableArray();

                logger.LogInformation("Checking conflicts for {Count} events", eventsToCheck.Count());

                var comp = new SemanticComparator(options.Value.ModelPath, options.Value.VocabPath);

                foreach (var data in eventsToCheck)
                {
                    logger.LogInformation("Checking overlaps for : {EventSummary}", data.Event.Summary);

                    var normalizedOriginal = SummaryNormalizationRegex().Replace(data.Event.Summary, "").Trim();

                    var overlappingEvents = targetEvents
                        .Where(e =>
                            e.Start.DateTimeDateTimeOffset < data.Event.End.DateTimeDateTimeOffset &&
                            e.End.DateTimeDateTimeOffset > data.Event.Start.DateTimeDateTimeOffset
                        )
                        .Select(e =>
                        {
                            var normalizedOverlap = SummaryNormalizationRegex().Replace(e.Summary, "").Trim();

                            var sim = comp.Compare(normalizedOriginal, normalizedOverlap);

                            return new
                            {
                                Similarity = sim,
                                Event = e
                            };
                        })
                        .Where(e => e.Similarity <= 0.40)
                        .Select(e => e.Event)
                        .ToImmutableArray();

                    if (!overlappingEvents.Any())
                    {
                        continue;
                    }

                    logger.LogInformation("Overlaps detected for {Summary} : {Count}", data.Event.Summary, overlappingEvents.Length);

                    data.Metadata.LastBdeComdEventSync = DateTimeOffset.UtcNow;
                    data.Event.ExtendedProperties.Shared[BdeComdSourceCalendarEventMetadata.Key] =
                        Convert.ToBase64String(
                            MemoryPackSerializer.Serialize(data.Metadata)
                        );

                    data.Event.Summary = "! " + data.Event.Summary;
                    data.Event.Description = $"""
                                              Conflicts with BC:

                                              {string.Join("\n", overlappingEvents.Select(e =>
                                                  $"- {e.Summary} ({e.Start.DateTimeDateTimeOffset:dd MMM yyyy HH:mm} - {e.End.DateTimeDateTimeOffset:dd MMM yyyy HH:mm})"
                                              ))}

                                              -- END --

                                              {data.Event.Description}
                                              """;

                    await calendarService.Events
                        .Update(data.Event, options.Value.SourceCalendarId, data.Event.Id)
                        .ExecuteAsync(ct);

                    logger.LogInformation("Updated event {Summary}", data.Event.Summary);
                }
            }, stoppingToken);

            await Task.Delay(options.Value.SyncInterval, stoppingToken);
        }
    }
}