using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clerk.BackendAPI;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Temasek.Auth.Options;

namespace Temasek.Auth.Features.FormSg.Validate.Callback;

public class Endpoint(IOptions<FormSgOptions> formSgOptions, ClerkBackendApi clerk) : Endpoint<Request>
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
         ValidIssuer = nameof(Index.Endpoint),
         ValidateIssuer = true,
         ValidateLifetime = true,
         IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes)
      });

      var userId = principal.ClaimsIdentity.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
      var user = await clerk.Users.GetAsync(userId);

      await clerk.Users.UpdateMetadataAsync(userId, new Clerk.BackendAPI.Models.Operations.UpdateUserMetadataRequestBody
      {
         PublicMetadata = new Dictionary<string, object>
        {
            { "nric", req.Nric },
            { "name", req.Name }
        }
      });

      await Send.OkAsync(ct);
   }
}
