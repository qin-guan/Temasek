using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace Temasek.Calendarr.Extensions;

public static class EventExtensions
{
    public static Event Clone(this Event @event)
    {
        return new Event
        {
            Id = @event.Id,
            Summary = @event.Summary,
            Status = @event.Status,
            Description = @event.Description,
            EventType = @event.EventType,
            Location = @event.Location,
            Recurrence = @event.Recurrence,
            Start = @event.Start,
            End = @event.End,
            EndTimeUnspecified = @event.EndTimeUnspecified,
            ColorId = @event.ColorId,
            Transparency = @event.Transparency,
            Visibility = @event.Visibility,
            GuestsCanInviteOthers = @event.GuestsCanInviteOthers,
            GuestsCanModify = @event.GuestsCanModify,
            GuestsCanSeeOtherGuests = @event.GuestsCanSeeOtherGuests,
            AnyoneCanAddSelf = @event.AnyoneCanAddSelf,
            Reminders = @event.Reminders,
            ExtendedProperties = @event.ExtendedProperties,
            Source = @event.Source,
            Sequence = @event.Sequence,
            PrivateCopy = @event.PrivateCopy,
            RecurringEventId = @event.RecurringEventId,
            OriginalStartTime = @event.OriginalStartTime,
            ConferenceData = @event.ConferenceData,
            Attachments = @event.Attachments,
            FocusTimeProperties = @event.FocusTimeProperties,
            OutOfOfficeProperties = @event.OutOfOfficeProperties,
            WorkingLocationProperties = @event.WorkingLocationProperties,
            BirthdayProperties = @event.BirthdayProperties,
            Attendees = [],
        };
    }

    public static async Task<List<Event>> ListAllAsync(
        this EventsResource resource,
        string calendarId,
        bool showDeleted = false,
        string? syncToken = null,
        CancellationToken ct = default
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

            if (syncToken is not null)
            {
                request.SyncToken = syncToken;
            }

            request.ShowDeleted = showDeleted;

            var results = await request.ExecuteAsync(ct);
            nextPageToken = results.NextPageToken;

            events.AddRange(
                results.Items.Select(e =>
                {
                    e.Attendees = [];
                    return e;
                })
            );
        } while (nextPageToken is not null);

        return events;
    }
}
