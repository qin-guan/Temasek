using Google.Apis.Calendar.v3;
using Microsoft.Extensions.Options;
using Temasek.Calendarr.Extensions;
using Temasek.Calendarr.Options;

namespace Temasek.Calendarr.Services;

public class CalendarSyncService(
    [FromKeyedServices("Sync")] CalendarService calendarService,
    ILogger<CalendarSyncService> logger,
    IOptions<SyncOptions> options
)
{
    public async Task RunFullSyncAsync(CancellationToken ct = default)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            ct,
            timeoutCts.Token
        );

        var token = linkedCts.Token;
        var parentEvents = await calendarService.Events.ListAllAsync(
            options.Value.ParentCalendarId,
            showDeleted: true,
            ct: token
        );
        var childEvents = await calendarService.Events.ListAllAsync(
            options.Value.ChildCalendarId,
            showDeleted: true,
            ct: token
        );
        var parentEventsLookup = parentEvents.ToDictionary(e => e.Id, e => e);
        var childEventsLookup = childEvents.ToDictionary(e => e.Id, e => e);

        foreach (var @event in parentEvents)
        {
            logger.LogInformation("Syncing event : {EventSummary}", @event.Summary);

            if (@event.Status == "cancelled")
            {
                if (childEventsLookup.ContainsKey(@event.Id))
                {
                    logger.LogInformation(
                        "Deleting event in child calendar (cancelled in parent) : {EventSummary}",
                        @event.Summary
                    );
                    await calendarService
                        .Events.Delete(options.Value.ChildCalendarId, @event.Id)
                        .ExecuteAsync(token);
                }
                continue;
            }

            if (childEventsLookup.ContainsKey(@event.Id))
            {
                logger.LogInformation(
                    "Updating event in child calendar : {EventSummary}",
                    @event.Summary
                );
                var updateRequest = calendarService.Events.Update(
                    @event.Clone(),
                    options.Value.ChildCalendarId,
                    @event.Id
                );
                updateRequest.SupportsAttachments = true;
                updateRequest.ConferenceDataVersion = 1;
                await updateRequest.ExecuteAsync(token);
                continue;
            }

            logger.LogInformation(
                "Inserting event in child calendar : {EventSummary}",
                @event.Summary
            );
            var insertRequest = calendarService.Events.Insert(
                @event.Clone(),
                options.Value.ChildCalendarId
            );
            insertRequest.SupportsAttachments = true;
            insertRequest.ConferenceDataVersion = 1;
            await insertRequest.ExecuteAsync(token);
        
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        foreach (var childEvent in childEvents)
        {
            if (parentEventsLookup.ContainsKey(childEvent.Id))
            {
                continue;
            }

            logger.LogInformation(
                "Deleting event in child calendar (missing in parent) : {EventSummary}",
                childEvent.Summary
            );
            await calendarService
                .Events.Delete(options.Value.ChildCalendarId, childEvent.Id)
                .ExecuteAsync(token);
        }
    }
}
