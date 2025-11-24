using FastEndpoints;
using SqlSugar;

namespace Temasek.Facilities.Endpoints.Booking.ById.Get;

public class Endpoint(ISqlSugarClient db) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/Booking/{Id:guid}");
    }

    public override async Task HandleAsync(Request req, CancellationToken ct) { }
}
