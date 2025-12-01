using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Temasek.Calendarr.Extensions;
using Temasek.Calendarr.Options;

namespace Temasek.Calendarr.Hubs;

public class SyncHub(
    [FromKeyedServices("Sync")] CalendarService calendarService,
    ILogger<SyncHub> logger,
    IOptions<SyncOptions> options
) : Hub
{
    public async Task RunFullSyncAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

        var ct = cts.Token;
        var parentEvents = await calendarService.Events.ListAllAsync(
            options.Value.ParentCalendarId,
            true,
            ct: ct
        );
        var childEvents = await calendarService.Events.ListAllAsync(
            options.Value.ChildCalendarId,
            true,
            ct: ct
        );
        var childEventsLookup = childEvents.ToDictionary(e => e.Id, e => e);

        foreach (var @event in parentEvents)
        {
            logger.LogInformation("Syncing event : {EventSummary}", @event.Summary);

            if (!childEventsLookup.TryGetValue(@event.Id, out var childEvent))
            {
                logger.LogInformation(
                    "Inserting event in child calendar : {EventSummary}",
                    @event.Summary
                );
                await calendarService
                    .Events.Insert(@event.Clone(), options.Value.ChildCalendarId)
                    .ExecuteAsync(ct);
                continue;
            }

            // If created in child and is updated in parent
            if (@event.Status != "cancelled")
            {
                logger.LogInformation(
                    "Updating event in child calendar : {EventSummary}",
                    @event.Summary
                );
                await calendarService
                    .Events.Update(@event.Clone(), options.Value.ChildCalendarId, @event.Id)
                    .ExecuteAsync(ct);
                continue;
            }

            // If created in child and is deleted in parent
            if (childEvent.Status != "cancelled")
            {
                logger.LogInformation(
                    "Deleting event in child calendar : {EventSummary}",
                    @event.Summary
                );
                await calendarService
                    .Events.Delete(options.Value.ChildCalendarId, @event.Id)
                    .ExecuteAsync(ct);
            }
        }
    }
}
