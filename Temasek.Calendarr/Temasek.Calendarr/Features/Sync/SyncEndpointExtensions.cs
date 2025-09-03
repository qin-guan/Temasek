using Google.Apis.Calendar.v3;
using Microsoft.Extensions.Options;
using Temasek.Calendarr.Options;

namespace Temasek.Calendarr.Features.Sync;

public static class SyncEndpointExtensions
{
    public static RouteGroupBuilder MapSync(this WebApplication app)
    {
        var group = app.MapGroup("sync");

        group.MapGet("force", async (
            [FromKeyedServices("Sync")] CalendarService calendarService,
            IOptions<SyncOptions> options,
            CancellationToken ct
        ) =>
        {
            var parentEvents = await calendarService.Events.ListAll(options.Value.ParentCalendarId, true, ct: ct);
            var childEvents = await calendarService.Events.ListAll(options.Value.ChildCalendarId, true, ct: ct);
            var childEventsLookup = childEvents.ToDictionary(e => e.Id, e => e);

            foreach (var @event in parentEvents)
            {
                app.Logger.LogInformation("Syncing event : {EventSummary}", @event.Summary);
                
                if (!childEventsLookup.TryGetValue(@event.Id, out var childEvent))
                {
                    app.Logger.LogInformation("Inserting event in child calendar : {EventSummary}", @event.Summary);
                    await calendarService.Events.Insert(@event.Clone(), options.Value.ChildCalendarId).ExecuteAsync(ct);
                    continue;
                }

                // If created in child and is updated in parent
                if (@event.Status != "cancelled")
                {
                    app.Logger.LogInformation("Updating event in child calendar : {EventSummary}", @event.Summary);
                    await calendarService.Events.Update(@event.Clone(), options.Value.ChildCalendarId, @event.Id)
                        .ExecuteAsync(ct);
                    continue;
                }

                // If created in child and is deleted in parent
                if (childEvent.Status != "cancelled")
                {
                    app.Logger.LogInformation("Deleting event in child calendar : {EventSummary}", @event.Summary);
                    await calendarService.Events.Delete(options.Value.ChildCalendarId, @event.Id)
                        .ExecuteAsync(ct);
                }
            }
        });

        return group;
    }
}