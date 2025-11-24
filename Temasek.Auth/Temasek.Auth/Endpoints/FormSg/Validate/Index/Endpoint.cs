using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Clerk.BackendAPI;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Temasek.Auth.Options;

namespace Temasek.Auth.Endpoints.FormSg.Validate.Index;

public class Endpoint(IOptions<FormSgOptions> formSgOptions) : EndpointWithoutRequest<Response>
{
    public const string Issuer = "Temasek.Auth.Endpoints.FormSg.Validate";

    private readonly byte[] secretKeyBytes = Encoding.UTF8.GetBytes(formSgOptions.Value.SecretKey);

    public override void Configure()
    {
        Get("FormSg/Validate");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Subject = User.Identities.First(),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKeyBytes),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        await Send.OkAsync(
            new Response
            {
                FormId = formSgOptions.Value.FormId,
                PrefillFieldId = formSgOptions.Value.PrefillFieldId,
                ClerkUserId = tokenHandler.WriteToken(token),
            },
            ct
        );
    }
}
