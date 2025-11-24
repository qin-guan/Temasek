using FastEndpoints;
using SqlSugar;

namespace Temasek.Facilities.Endpoints.Booking.ById.Post;

public class Endpoint(ISqlSugarClient db) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/Booking/{Id:guid}");
    }

    public override async Task HandleAsync(Request req, CancellationToken ct) { }
}
