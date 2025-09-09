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
            SyncService syncService,
            CancellationToken ct
        ) =>
        {
            await syncService.ForceSync(ct);
            return TypedResults.Ok();
        });

        return group;
    }
}