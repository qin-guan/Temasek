using FastEndpoints;

namespace Temasek.Facilities.Endpoints.Facility.List;

public class Endpoint : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("facility");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(
            new Response
            {
                Facilities = Entities.Facility.All.Select(f => new FacilityItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    Group = f.Group.Value,
                    Scope = f.Scope.Select(s => s.Value),
                }),
            },
            ct
        );
    }
}
