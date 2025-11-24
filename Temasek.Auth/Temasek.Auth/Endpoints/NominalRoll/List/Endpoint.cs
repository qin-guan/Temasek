using FastEndpoints;

namespace Temasek.Auth.Endpoints.NominalRoll.List;

public class Endpoint(ILogger<Endpoint> loggerk) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("NominalRoll");
    }

    public override async Task HandleAsync(CancellationToken ct) { }
}
