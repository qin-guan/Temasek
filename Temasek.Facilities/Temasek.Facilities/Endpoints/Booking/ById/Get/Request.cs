using FastEndpoints;

namespace Temasek.Facilities.Endpoints.Booking.ById.Get;

public class Request
{
    [RouteParam]
    public Guid Id { get; set; }
}
