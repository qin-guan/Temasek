using Google.Apis.Calendar.v3;
using Microsoft.Extensions.Options;
using Polly;
using Temasek.Calendarr.Extensions;
using Temasek.Calendarr.Options;

namespace Temasek.Calendarr.Workers;

public class SyncIncrementalBackgroundService(
    [FromKeyedServices("Sync")] CalendarService calendarService,
    [FromKeyedServices("BackgroundService")] ResiliencePipeline pipeline,
    ILogger<SyncIncrementalBackgroundService> logger,
    IOptions<SyncOptions> options
) : BackgroundService
{
    private string? _syncToken;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting worker");

        while (!stoppingToken.IsCancellationRequested)
        {
            await pipeline.ExecuteAsync(
                async ct =>
                {
                    var tokenReq = calendarService.Events.List(options.Value.ParentCalendarId);
                    tokenReq.TimeMinDateTimeOffset = DateTimeOffset.Now.AddDays(-7);
                    tokenReq.MaxResults = 2499;

                    var tokenRes = await tokenReq.ExecuteAsync(ct);
                    _syncToken = tokenRes.NextSyncToken;
                    logger.LogInformation("Running sync with token : {SyncToken}", _syncToken);

                    var changedEvents = await calendarService.Events.ListAllAsync(
                        options.Value.ParentCalendarId,
                        showDeleted: true,
                        syncToken: _syncToken,
                        ct: ct
                    );

                    if (changedEvents.Count == 0)
                    {
                        logger.LogInformation("No changes since last update");
                        return;
                    }

                    logger.LogInformation(
                        "Syncing changes since last update : {Diff}",
                        changedEvents.Count
                    );
                    var childEvents = await calendarService.Events.ListAllAsync(
                        options.Value.ChildCalendarId,
                        ct: stoppingToken
                    );
                    var childEventsLookup = childEvents.ToDictionary(e => e.Id, e => e);

                    foreach (var @event in changedEvents)
                    {
                        if (@event.Status == "cancelled")
                        {
                            logger.LogInformation(
                                "Deleting event in child calendar : {EventSummary}",
                                @event.Summary
                            );
                            await calendarService
                                .Events.Delete(options.Value.ChildCalendarId, @event.Id)
                                .ExecuteAsync(ct);
                        }

                        if (childEventsLookup.ContainsKey(@event.Id))
                        {
                            logger.LogInformation(
                                "Updating event in child calendar : {EventSummary}",
                                @event.Summary
                            );
                            await calendarService
                                .Events.Update(
                                    @event.Clone(),
                                    options.Value.ChildCalendarId,
                                    @event.Id
                                )
                                .ExecuteAsync(ct);
                        }
                        else
                        {
                            logger.LogInformation(
                                "Inserting event in child calendar : {EventSummary}",
                                @event.Summary
                            );
                            await calendarService
                                .Events.Insert(@event.Clone(), options.Value.ChildCalendarId)
                                .ExecuteAsync(ct);
                        }
                    }
                },
                stoppingToken
            );

            await Task.Delay(options.Value.SyncInterval, stoppingToken);
        }
    }
}
