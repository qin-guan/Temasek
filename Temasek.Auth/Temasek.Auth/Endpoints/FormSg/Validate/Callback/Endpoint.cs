using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clerk.BackendAPI;
using Clerk.BackendAPI.Models.Operations;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Temasek.Auth.Options;

namespace Temasek.Auth.Endpoints.FormSg.Validate.Callback;

public class Endpoint(ILogger<Endpoint> logger, IOptions<FormSgOptions> formSgOptions, ClerkBackendApi clerk) : Endpoint<Request>
{
    private readonly byte[] secretKeyBytes = Encoding.UTF8.GetBytes(formSgOptions.Value.SecretKey);

    public override void Configure()
    {
        Post("FormSg/Validate/Callback");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        if (HttpContext.Request.Headers["X-API-KEY"] != formSgOptions.Value.CallbackApiKey)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = await tokenHandler.ValidateTokenAsync(req.ClerkUserId, new TokenValidationParameters
        {
            // I don't think this would cause any security issues since we only need to get the data
            ValidateAudience = false,
            ValidIssuer = Index.Endpoint.Issuer,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes)
        });

        if (principal.IsValid == false)
        {
            logger.LogWarning("Failed to verify Clerk User ID token: {Message}", principal.Exception.Message);
            await Send.ForbiddenAsync(ct);
            return;
        }

        var userId = principal.ClaimsIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            logger.LogWarning("Clerk User ID token does not contain NameIdentifier claim");
            await Send.ForbiddenAsync(cancellation: ct);
            return;
        }

        var user = await clerk.Users.GetAsync(userId);

        await clerk.Users.UpdateMetadataAsync(userId, new UpdateUserMetadataRequestBody
        {
            PublicMetadata = new Dictionary<string, object>
        {
            { "nric", req.Nric },
            { "name", req.Name }
        }
        });

        await Send.OkAsync(cancellation: ct);
    }
}
