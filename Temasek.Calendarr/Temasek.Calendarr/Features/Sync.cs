using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using Temasek.Calendarr.Options;

namespace Temasek.Calendarr.Features;

public static class SyncExtensions
{
    public static RouteGroupBuilder MapSync(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("sync");

        group.MapGet("force", async (
            [FromKeyedServices("Sync")] CalendarService calendarService,
            IOptions<SyncOptions> options,
            CancellationToken ct
        ) =>
        {
            var primaryEvents = await calendarService.Events.ListAllEvents(options.Value.PrimaryCalendarId, false, ct);
            var secondaryEvents =
                await calendarService.Events.ListAllEvents(options.Value.SecondaryCalendarId, true, ct);

            await Parallel.ForEachAsync(
                secondaryEvents,
                new ParallelOptions
                {
                    CancellationToken = ct,
                    MaxDegreeOfParallelism = 1,
                },
                async (se, ct2) =>
                {
                    Console.WriteLine($"Deleting {se.Summary}");
                    if (se.Status == "cancelled")
                    {
                        Console.WriteLine($"Skipping {se.Summary}");
                    }
                    else
                    {
                        Console.WriteLine($"Deleting {se.Summary}");
                        await calendarService.Events.Delete(options.Value.SecondaryCalendarId, se.Id).ExecuteAsync(ct2);
                    }

                    await Task.Delay(1000, ct2);
                });

            await Parallel.ForEachAsync(
                primaryEvents,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 1,
                    CancellationToken = ct,
                },
                async (se, ct2) =>
                {
                    Console.WriteLine($"Adding {se.Summary}");
                    se.Attendees = [];

                    var exists = secondaryEvents.SingleOrDefault(e => e.Id == se.Id);
                    if (exists is not null)
                    {
                        exists.Status = "confirmed";
                        exists.Summary = se.Summary;
                        exists.Start = se.Start;
                        exists.End = se.End;
                        exists.Location = se.Location;
                        exists.Attendees = [];
                        Console.WriteLine($"Updating {se.Summary}");
                        await calendarService.Events.Update(exists, options.Value.SecondaryCalendarId, se.Id)
                            .ExecuteAsync(ct2);
                    }
                    else
                    {
                        Console.WriteLine($"Creating {se.Summary}");
                        await calendarService.Events.Insert(se, options.Value.SecondaryCalendarId).ExecuteAsync(ct2);
                    }

                    await Task.Delay(1000, ct2);
                });
        });

        group.MapGet("delete/{id:required}", (string id) =>
        {

        });
        
        group.MapGet("update/{id:required}", (string id) =>
        {

        });
        
        group.MapGet("insert/{id:required}", (string id) =>
        {
            
        });

        return group;
    }

    private static async Task<List<Event>> ListAllEvents(
        this EventsResource resource,
        string calendarId,
        bool showDeleted,
        CancellationToken ct
    )
    {
        var events = new List<Event>();
        string? nextPageToken = null;

        do
        {
            var request = resource.List(calendarId);
            if (nextPageToken is not null)
            {
                request.PageToken = nextPageToken;
            }

            request.ShowDeleted = showDeleted;

            var results = await request.ExecuteAsync(ct);
            nextPageToken = results.NextPageToken;

            events.AddRange(results.Items);
        } while (nextPageToken is not null);

        return events;
    }
}