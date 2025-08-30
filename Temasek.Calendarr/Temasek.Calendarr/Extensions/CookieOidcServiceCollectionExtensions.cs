using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Temasek.Calendarr.Services;

namespace Temasek.Calendarr.Extensions;

internal static partial class CookieOidcServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCookieOidc(
        this IServiceCollection services,
        string cookieScheme,
        string oidcScheme
    )
    {
        services.AddSingleton<CookieOidcRefresherService>();
        
        services
            .AddOptions<CookieAuthenticationOptions>(cookieScheme)
            .Configure<CookieOidcRefresherService>((cookieOptions,
                refresher) =>
            {
                cookieOptions.Events.OnValidatePrincipal = context =>
                    refresher.ValidateOrRefreshCookieAsync(context,
                        oidcScheme);
            });
        services
            .AddOptions<OpenIdConnectOptions>(oidcScheme)
            .Configure(oidcOptions => { oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess); });
        
        return services;
    }
}