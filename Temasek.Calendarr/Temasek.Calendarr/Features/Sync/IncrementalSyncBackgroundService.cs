using Google.Apis.Calendar.v3;
using Microsoft.Extensions.Options;
using Temasek.Calendarr.Options;

namespace Temasek.Calendarr.Features.Sync;

public class IncrementalSyncBackgroundService(
    [FromKeyedServices("Sync")] CalendarService calendarService,
    ILogger<IncrementalSyncBackgroundService> logger,
    IOptions<SyncOptions> options
) : BackgroundService
{
    private string? _syncToken;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting worker");

        while (!stoppingToken.IsCancellationRequested)
        {
            var tokenReq = calendarService.Events.List(options.Value.ParentCalendarId);
            tokenReq.MaxResults = 1;

            var tokenRes = await tokenReq.ExecuteAsync(stoppingToken);
            _syncToken = tokenRes.NextSyncToken;
            logger.LogInformation("Retrieved sync token : {SyncToken}", _syncToken);

            await Task.Delay(options.Value.SyncInterval, stoppingToken);
            logger.LogInformation("Running sync with token : {SyncToken}", _syncToken);

            var changedEvents = await calendarService.Events.ListAll(
                options.Value.ParentCalendarId,
                showDeleted: true,
                syncToken: _syncToken,
                ct: stoppingToken
            );

            if (changedEvents.Count != 0)
            {
                logger.LogInformation("No changes since last update");
                continue;
            }

            logger.LogInformation("Syncing changes since last update : {Diff}", changedEvents.Count);
            var childEvents = await calendarService.Events.ListAll(options.Value.ChildCalendarId, ct: stoppingToken);
            var childEventsLookup = childEvents.ToDictionary(e => e.Id, e => e);

            foreach (var @event in changedEvents)
            {
                if (@event.Status == "cancelled")
                {
                    logger.LogInformation("Deleting event in child calendar : {EventSummary}", @event.Summary);
                    await calendarService.Events.Delete(options.Value.ChildCalendarId, @event.Id)
                        .ExecuteAsync(stoppingToken);
                }

                if (childEventsLookup.ContainsKey(@event.Id))
                {
                    logger.LogInformation("Updating event in child calendar : {EventSummary}", @event.Summary);
                    await calendarService.Events.Update(@event.Clone(), options.Value.ChildCalendarId, @event.Id)
                        .ExecuteAsync(stoppingToken);
                }
                else
                {
                    logger.LogInformation("Inserting event in child calendar : {EventSummary}", @event.Summary);
                    await calendarService.Events.Insert(@event.Clone(), options.Value.ChildCalendarId)
                        .ExecuteAsync(stoppingToken);
                }
            }
        }
    }
}