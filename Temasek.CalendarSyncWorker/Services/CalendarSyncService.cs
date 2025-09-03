using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using Temasek.CalendarSyncWorker.Options;

namespace Temasek.CalendarSyncWorker.Services;

public class CalendarSyncService
{
    private readonly CalendarService _calendarService;
    private readonly CalendarSyncOptions _options;
    private readonly ILogger<CalendarSyncService> _logger;
    private string? _primarySyncToken;
    private DateTime _lastFullSync = DateTime.MinValue;

    public CalendarSyncService(
        CalendarService calendarService, 
        IOptions<CalendarSyncOptions> options,
        ILogger<CalendarSyncService> logger)
    {
        _calendarService = calendarService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Performs incremental synchronization between primary and secondary calendars
    /// </summary>
    public async Task SyncCalendarsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting calendar synchronization...");

            // Perform full sync on startup or if no sync token exists
            if (_options.PerformFullSyncOnStartup && _primarySyncToken == null)
            {
                await PerformFullSyncAsync(cancellationToken);
                return;
            }

            // Perform incremental sync using sync token
            await PerformIncrementalSyncAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during calendar synchronization");
            throw;
        }
    }

    private async Task PerformFullSyncAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Performing full calendar synchronization...");

        var primaryEvents = await GetAllEventsAsync(_options.PrimaryCalendarId, cancellationToken);
        var secondaryEvents = await GetAllEventsAsync(_options.SecondaryCalendarId, cancellationToken);

        _logger.LogInformation("Found {PrimaryCount} events in primary calendar and {SecondaryCount} in secondary",
            primaryEvents.Count, secondaryEvents.Count);

        // Create lookup for secondary events by their original ID for faster matching
        var secondaryEventLookup = secondaryEvents
            .Where(e => !string.IsNullOrEmpty(e.Id))
            .ToDictionary(e => e.Id!, e => e);

        var syncTasks = new List<Task>();
        var semaphore = new SemaphoreSlim(3, 3); // Limit concurrent operations

        foreach (var primaryEvent in primaryEvents)
        {
            if (cancellationToken.IsCancellationRequested) break;

            syncTasks.Add(SyncSingleEventAsync(primaryEvent, secondaryEventLookup, semaphore, cancellationToken));
        }

        await Task.WhenAll(syncTasks);

        // Clean up events that exist in secondary but not in primary
        await RemoveOrphanedEventsAsync(primaryEvents, secondaryEvents, cancellationToken);

        _lastFullSync = DateTime.UtcNow;
        _logger.LogInformation("Full synchronization completed successfully");

        // Get fresh sync token for future incremental syncs
        await InitializeSyncTokenAsync(cancellationToken);
    }

    private async Task PerformIncrementalSyncAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_primarySyncToken))
        {
            _logger.LogWarning("No sync token available, falling back to full sync");
            await PerformFullSyncAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Performing incremental calendar synchronization...");

        try
        {
            var changedEvents = await GetChangedEventsAsync(_primarySyncToken, cancellationToken);
            
            if (!changedEvents.Any())
            {
                _logger.LogInformation("No changes detected in primary calendar");
                return;
            }

            _logger.LogInformation("Processing {Count} changed events", changedEvents.Count);

            var secondaryEvents = await GetAllEventsAsync(_options.SecondaryCalendarId, cancellationToken);
            var secondaryEventLookup = secondaryEvents.ToDictionary(e => e.Id!, e => e);

            var semaphore = new SemaphoreSlim(3, 3);
            var syncTasks = changedEvents.Select(evt => 
                SyncSingleEventAsync(evt, secondaryEventLookup, semaphore, cancellationToken));

            await Task.WhenAll(syncTasks);

            _logger.LogInformation("Incremental synchronization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during incremental sync, will retry with full sync");
            _primarySyncToken = null; // Reset sync token to force full sync next time
            throw;
        }
    }

    private async Task<List<Event>> GetChangedEventsAsync(string syncToken, CancellationToken cancellationToken)
    {
        var events = new List<Event>();
        string? nextPageToken = null;
        string? newSyncToken = null;

        do
        {
            var request = _calendarService.Events.List(_options.PrimaryCalendarId);
            request.SyncToken = syncToken;
            request.ShowDeleted = true;
            
            if (!string.IsNullOrEmpty(nextPageToken))
            {
                request.PageToken = nextPageToken;
            }

            var response = await request.ExecuteAsync(cancellationToken);
            
            if (response.Items != null)
            {
                events.AddRange(response.Items);
            }

            nextPageToken = response.NextPageToken;
            if (!string.IsNullOrEmpty(response.NextSyncToken))
            {
                newSyncToken = response.NextSyncToken;
            }

        } while (!string.IsNullOrEmpty(nextPageToken));

        // Update sync token for next incremental sync
        if (!string.IsNullOrEmpty(newSyncToken))
        {
            _primarySyncToken = newSyncToken;
        }

        return events;
    }

    private async Task SyncSingleEventAsync(
        Event primaryEvent, 
        Dictionary<string, Event> secondaryEventLookup,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        
        try
        {
            if (primaryEvent.Status == "cancelled")
            {
                await HandleEventDeletionAsync(primaryEvent.Id!, cancellationToken);
                return;
            }

            if (secondaryEventLookup.TryGetValue(primaryEvent.Id!, out var existingEvent))
            {
                await UpdateEventAsync(primaryEvent, existingEvent, cancellationToken);
            }
            else
            {
                await CreateEventAsync(primaryEvent, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing event {EventId}: {EventTitle}", 
                primaryEvent.Id, primaryEvent.Summary);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task CreateEventAsync(Event primaryEvent, CancellationToken cancellationToken)
    {
        var eventCopy = CloneEventForSync(primaryEvent);
        
        _logger.LogDebug("Creating event: {EventTitle}", primaryEvent.Summary);
        
        await _calendarService.Events.Insert(eventCopy, _options.SecondaryCalendarId)
            .ExecuteAsync(cancellationToken);
        
        _logger.LogInformation("Created event: {EventTitle}", primaryEvent.Summary);
    }

    private async Task UpdateEventAsync(Event primaryEvent, Event existingEvent, CancellationToken cancellationToken)
    {
        var eventCopy = CloneEventForSync(primaryEvent);
        eventCopy.Id = existingEvent.Id; // Keep the existing ID
        
        _logger.LogDebug("Updating event: {EventTitle}", primaryEvent.Summary);
        
        await _calendarService.Events.Update(eventCopy, _options.SecondaryCalendarId, existingEvent.Id!)
            .ExecuteAsync(cancellationToken);
        
        _logger.LogInformation("Updated event: {EventTitle}", primaryEvent.Summary);
    }

    private async Task HandleEventDeletionAsync(string eventId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Deleting event: {EventId}", eventId);
            
            await _calendarService.Events.Delete(_options.SecondaryCalendarId, eventId)
                .ExecuteAsync(cancellationToken);
            
            _logger.LogInformation("Deleted event: {EventId}", eventId);
        }
        catch (Exception ex) when (ex.Message.Contains("404"))
        {
            // Event doesn't exist in secondary calendar, which is fine
            _logger.LogDebug("Event {EventId} not found in secondary calendar (already deleted)", eventId);
        }
    }

    private async Task<List<Event>> GetAllEventsAsync(string calendarId, CancellationToken cancellationToken)
    {
        var events = new List<Event>();
        string? nextPageToken = null;

        do
        {
            var request = _calendarService.Events.List(calendarId);
            request.MaxResults = _options.MaxBatchSize;
            request.ShowDeleted = false;
            
            if (!string.IsNullOrEmpty(nextPageToken))
            {
                request.PageToken = nextPageToken;
            }

            var response = await request.ExecuteAsync(cancellationToken);
            
            if (response.Items != null)
            {
                events.AddRange(response.Items);
            }

            nextPageToken = response.NextPageToken;
        } while (!string.IsNullOrEmpty(nextPageToken));

        return events;
    }

    private async Task RemoveOrphanedEventsAsync(
        List<Event> primaryEvents, 
        List<Event> secondaryEvents, 
        CancellationToken cancellationToken)
    {
        var primaryEventIds = primaryEvents.Select(e => e.Id).ToHashSet();
        var orphanedEvents = secondaryEvents.Where(e => !primaryEventIds.Contains(e.Id)).ToList();

        if (!orphanedEvents.Any())
        {
            return;
        }

        _logger.LogInformation("Removing {Count} orphaned events from secondary calendar", orphanedEvents.Count);

        var semaphore = new SemaphoreSlim(3, 3);
        var deleteTasks = orphanedEvents.Select(evt => 
            DeleteOrphanedEventAsync(evt.Id!, semaphore, cancellationToken));

        await Task.WhenAll(deleteTasks);
    }

    private async Task DeleteOrphanedEventAsync(string eventId, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        
        try
        {
            await HandleEventDeletionAsync(eventId, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task InitializeSyncTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = _calendarService.Events.List(_options.PrimaryCalendarId);
            request.MaxResults = 1; // We only need the sync token
            
            var response = await request.ExecuteAsync(cancellationToken);
            _primarySyncToken = response.NextSyncToken;
            
            _logger.LogInformation("Initialized sync token for future incremental syncs");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize sync token");
        }
    }

    private static Event CloneEventForSync(Event source)
    {
        return new Event
        {
            Id = source.Id,
            Summary = source.Summary,
            Description = source.Description,
            Start = source.Start,
            End = source.End,
            Location = source.Location,
            Status = "confirmed",
            Visibility = source.Visibility,
            Transparency = source.Transparency,
            Attendees = new List<EventAttendee>(), // Clear attendees for privacy
            Reminders = source.Reminders,
            RecurringEventId = source.RecurringEventId,
            Recurrence = source.Recurrence
        };
    }
}