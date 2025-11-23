using FastEndpoints;

namespace Temasek.Facilities.Endpoints.Booking.ById.Delete;

public class Request
{
    [RouteParam]
    public Guid Id { get; set; }
}
