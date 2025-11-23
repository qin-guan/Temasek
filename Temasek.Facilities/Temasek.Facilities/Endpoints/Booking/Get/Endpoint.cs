using FastEndpoints;
using SqlSugar;

namespace Temasek.Facilities.Endpoints.Booking.Get;

public class Endpoint(
    ISqlSugarClient db
) : EndpointWithoutRequest<IEnumerable<Response>>
{
    public override void Configure()
    {
        Get("/Booking");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
    }
}
