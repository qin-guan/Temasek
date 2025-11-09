using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Temasek.Auth.Data;
using Temasek.Auth.Options;

namespace Temasek.Auth.Workers;

public class OpenIddictBackgroundService(IServiceProvider serviceProvider, IOptions<OpenIddictOptions> options)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(stoppingToken);

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        foreach (var clientOptions in options.Value.Clients)
        {
            var client = new OpenIddictApplicationDescriptor
            {
                ClientType = OpenIddictConstants.ClientTypes.Public,
                ClientId = clientOptions.ClientId,
                ClientSecret = clientOptions.ClientSecret,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.Endpoints.PushedAuthorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,

                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,

                    OpenIddictConstants.Permissions.ResponseTypes.Code,

                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                }
            };

            foreach (var uri in clientOptions.RedirectUris)
            {
                client.RedirectUris.Add(uri);
            }

            var app = await manager.FindByClientIdAsync(clientOptions.ClientId, stoppingToken);
            if (app is not null)
            {
                await manager.UpdateAsync(app, client, stoppingToken);
            }
            else
            {
                await manager.CreateAsync(client, stoppingToken);
            }
        }
    }
}