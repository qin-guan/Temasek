using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Temasek.Auth.Data;
using Temasek.Auth.Options;

namespace Temasek.Auth.Features;

public record Callback(Guid SessionId, string Nric, string Name);

public static class Oidc
{
    private const string SessionAuthenticationKey = "SessionAuthentication";

    public static IEndpointRouteBuilder MapOidcEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("~/callback/login/formsg",
            async (
                [FromBody] Callback callback,
                [FromHeader(Name = "X-API-KEY")] string apiKey,
                IOptions<FormGovSgOptions> options,
                ApplicationDbContext dbContext,
                CancellationToken ct
            ) =>
            {
                if (options.Value.CallbackApiKey != apiKey)
                {
                    return Results.Unauthorized();
                }

                var session = await dbContext.SessionAuthentications
                    .SingleAsync(s => s.Id == callback.SessionId, cancellationToken: ct);

                session.Nric = callback.Nric;
                session.Name = callback.Name;
                dbContext.Update(session);

                await dbContext.SaveChangesAsync(ct);

                return Results.Ok();
            });

        app.MapMethods("~/login/formsg-redirect",
            [HttpMethods.Get, HttpMethods.Post],
            async (HttpContext context, ApplicationDbContext dbContext, CancellationToken ct) =>
            {
                var sessionIdString = context.Session.GetString(SessionAuthenticationKey);
                if (sessionIdString is null)
                {
                    return Results.LocalRedirect("~/connect/authorize");
                }

                if (!Guid.TryParse(sessionIdString, out var sessionId))
                {
                    return Results.NotFound();
                }

                var session = await dbContext.SessionAuthentications
                    .SingleAsync(s => s.Id == sessionId, cancellationToken: ct);

                return Results.LocalRedirect($"~/connect/authorize{session.Qs}");
            });

        app.MapMethods("~/connect/authorize",
            [HttpMethods.Get, HttpMethods.Post],
            async (HttpContext context, ApplicationDbContext dbContext, CancellationToken ct) =>
            {
                var sessionIdString = context.Session.GetString(SessionAuthenticationKey);

                if (sessionIdString is null)
                {
                    var entity = await dbContext.SessionAuthentications.AddAsync(
                        new SessionAuthentication
                        {
                            Qs = context.Request.QueryString.Value ?? string.Empty
                        },
                        ct
                    );
                    await dbContext.SaveChangesAsync(ct);

                    context.Session.SetString(SessionAuthenticationKey, entity.Entity.Id.ToString());

                    return TypedResults.Redirect(
                        $"https://form.gov.sg/6854b30b6a00d23824a0c5c7?6854e7ed87c7ea5ac8fff962={entity.Entity.Id}"
                    );
                }

                var session = await dbContext.SessionAuthentications
                    .SingleAsync(s => s.Id == Guid.Parse(sessionIdString), cancellationToken: ct);

                var request = context.GetOpenIddictServerRequest();

                if (session.Name is null || session.Nric is null || request is null)
                {
                    return TypedResults.BadRequest("You already have an authentication session in progress. Please clear your cookies, or wait 5 minutes, then try again.");
                }

                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    OpenIddictConstants.Claims.Name,
                    OpenIddictConstants.Claims.Role
                );

                identity
                    .SetClaim(
                        OpenIddictConstants.Claims.Subject,
                        session.Nric
                    )
                    .SetClaim(
                        OpenIddictConstants.Claims.GivenName,
                        session.Name
                    );

                identity.SetDestinations(GetDestinations);

                context.Session.Clear();
                dbContext.Remove(session);

                await dbContext.SaveChangesAsync(ct);

                var principal = new ClaimsPrincipal(identity);

                principal.SetScopes(request.GetScopes());

                return Results.SignIn(
                    principal,
                    properties: null,
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );
            });

        app.MapPost(
            "~/connect/token",
            async (HttpContext httpContext, IOpenIddictApplicationManager applicationManager, CancellationToken ct) =>
            {
                var request = httpContext.GetOpenIddictServerRequest();
                if (request?.ClientId is null)
                {
                    return TypedResults.BadRequest();
                }

                if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
                {
                    return TypedResults.BadRequest("The specified grant type is not supported.");
                }

                var result =
                    await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                _ = await applicationManager.FindByClientIdAsync(request.ClientId, ct) ??
                    throw new InvalidOperationException("The application cannot be found.");

                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    OpenIddictConstants.Claims.Name,
                    OpenIddictConstants.Claims.Role
                );

                // In most cases, it would be desirable to fetch and populate the claims with updated user values
                // However, we do not as this server does store any user information
                if (result.Principal?.Claims != null)
                {
                    identity.AddClaims(result.Principal.Claims);
                }

                identity.SetDestinations(GetDestinations);

                var principal = new ClaimsPrincipal(identity);

                principal.SetScopes(request.GetScopes());

                return Results.SignIn(
                    principal,
                    authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );
            });

        return app;
    }

    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            OpenIddictConstants.Claims.GivenName when
                claim.Subject!.HasScope(OpenIddictConstants.Permissions.Scopes.Profile) =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken,
                ],

            OpenIddictConstants.Claims.Subject when
                claim.Subject!.HasScope(OpenIddictConstants.Permissions.Scopes.Profile) =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken,
                ],

            OpenIddictConstants.Claims.Email when
                claim.Subject!.HasScope(OpenIddictConstants.Permissions.Scopes.Profile) =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken,
                ],

            _ =>
            [
                OpenIddictConstants.Destinations.AccessToken
            ]
        };
    }
}