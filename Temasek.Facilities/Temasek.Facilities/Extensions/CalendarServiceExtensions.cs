using System.Runtime.CompilerServices;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace Temasek.Facilities.Extensions;

public static class CalendarServiceExtensions
{
    extension(EventsResource events)
    {
        public async IAsyncEnumerable<Event> ListAllAsync(
            string calendarId,
            [EnumeratorCancellation] CancellationToken ct = default
        )
        {
            string? pageToken = null;

            do
            {
                var req = events.List(calendarId);
                if (pageToken is not null)
                {
                    req.PageToken = pageToken;
                }

                req.MaxResults = 2500;

                var res = await req.ExecuteAsync(ct);
                if (res.Items != null)
                {
                    foreach (var resItem in res.Items)
                        yield return resItem;
                }

                pageToken = res.NextPageToken;
            } while (pageToken != null);
        }
    }
}
