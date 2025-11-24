using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
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

public partial class BdeComdClearChangesBackgroundWorker(
    [FromKeyedServices("BdeComd")] CalendarService calendarService,
    [FromKeyedServices("BackgroundService")] ResiliencePipeline pipeline,
    ILogger<BdeComdClearChangesBackgroundWorker> logger,
    IOptions<BdeComdOptions> options
) : BackgroundService
{
    [GeneratedRegex(@"\[.*((?i)((bc)|(all))).*\]")]
    private static partial Regex BcSummaryRegex();

    [GeneratedRegex(@"Conflicts with BC:(.|\n)*-- END --\n*")]
    private static partial Regex BcClearDescriptionRegex();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting worker");

        while (!stoppingToken.IsCancellationRequested)
        {
            await pipeline.ExecuteAsync(
                async (svc, ct) =>
                {
                    var events = await svc.calendarService.Events.ListAllAsync(
                        options.Value.SourceCalendarId,
                        ct: ct
                    );

                    var eventsToClear = events
                        .Where(e => BcSummaryRegex().IsMatch(e.Summary))
                        .Where(e => e.Summary.StartsWith("! "))
                        .Where(e =>
                            e.ExtendedProperties?.Shared?[BdeComdSourceCalendarEventMetadata.Key]
                                is not null
                        )
                        .ToImmutableArray();

                    logger.LogInformation(
                        "Clearing changes for {Count} events",
                        eventsToClear.Length
                    );

                    foreach (var @event in eventsToClear)
                    {
                        logger.LogInformation(
                            "Clearing modified event : {Summary}",
                            @event.Summary
                        );

                        @event.Summary = @event.Summary.Replace("! ", "");
                        @event.ExtendedProperties.Shared = new Dictionary<string, string>();
                        @event.Description = BcClearDescriptionRegex()
                            .Replace(@event.Description, "");

                        await svc
                            .calendarService.Events.Update(
                                @event,
                                options.Value.SourceCalendarId,
                                @event.Id
                            )
                            .ExecuteAsync(ct);

                        logger.LogInformation(
                            "Completed clearing event : {Summary}",
                            @event.Summary
                        );
                    }
                },
                new { calendarService },
                stoppingToken
            );

            await Task.Delay(options.Value.SyncInterval, stoppingToken);
        }
    }
}
