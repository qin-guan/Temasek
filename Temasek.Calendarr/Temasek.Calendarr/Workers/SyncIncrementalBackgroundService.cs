using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
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
                    if (_syncToken is null)
                    {
                        logger.LogInformation(
                            "Bootstrapping incremental sync with 7-day lookback"
                        );
                    }
                    else
                    {
                        logger.LogInformation("Running sync with token : {SyncToken}", _syncToken);
                    }

                    var (changedEvents, nextSyncToken) = await ListChangedEventsAsync(ct);
                    if (!string.IsNullOrWhiteSpace(nextSyncToken))
                    {
                        _syncToken = nextSyncToken;
                    }

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
                        showDeleted: true,
                        ct: ct
                    );
                    var childEventsLookup = childEvents.ToDictionary(e => e.Id, e => e);

                    foreach (var @event in changedEvents)
                    {
                        if (@event.Status == "cancelled")
                        {
                            if (childEventsLookup.ContainsKey(@event.Id))
                            {
                                logger.LogInformation(
                                    "Deleting event in child calendar : {EventSummary}",
                                    @event.Summary
                                );
                                await calendarService
                                    .Events.Delete(options.Value.ChildCalendarId, @event.Id)
                                    .ExecuteAsync(ct);
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
                            await updateRequest.ExecuteAsync(ct);
                        }
                        else
                        {
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
                            await insertRequest.ExecuteAsync(ct);
                        }
                    }
                },
                stoppingToken
            );

            await Task.Delay(options.Value.SyncInterval, stoppingToken);
        }
    }

    private async Task<(List<Event> Events, string? NextSyncToken)> ListChangedEventsAsync(
        CancellationToken ct
    )
    {
        var events = new List<Event>();
        string? nextPageToken = null;
        string? nextSyncToken = null;

        do
        {
            var request = calendarService.Events.List(options.Value.ParentCalendarId);
            request.ShowDeleted = true;
            request.MaxResults = 2499;

            if (_syncToken is null)
            {
                request.TimeMinDateTimeOffset = DateTimeOffset.Now.AddDays(-7);
            }
            else
            {
                request.SyncToken = _syncToken;
            }

            if (nextPageToken is not null)
            {
                request.PageToken = nextPageToken;
            }

            var response = await request.ExecuteAsync(ct);
            nextPageToken = response.NextPageToken;
            nextSyncToken = response.NextSyncToken ?? nextSyncToken;

            if (response.Items is not null)
            {
                events.AddRange(
                    response.Items.Select(e =>
                    {
                        e.Attendees = [];
                        return e;
                    })
                );
            }
        } while (nextPageToken is not null);

        return (events, nextSyncToken);
    }
}
