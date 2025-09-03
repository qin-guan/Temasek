using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Temasek.Calendarr.Components;
using Temasek.Calendarr.Extensions;
using Temasek.Calendarr.Features;
using Temasek.Calendarr.Features.Sync;
using Temasek.Calendarr.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<SyncOptions>()
    .Bind(builder.Configuration.GetSection("Sync"));

builder.Services.AddKeyedSingleton<CalendarService>("Sync", (sp, _) =>
{
    var options = sp.GetRequiredService<IOptions<SyncOptions>>();
    var s = Convert.FromBase64String(options.Value.ServiceAccountJsonCredential);

    var credential = GoogleCredential.FromJson(Encoding.UTF8.GetString(s)).CreateScoped(
        "https://www.googleapis.com/auth/calendar",
        "https://www.googleapis.com/auth/calendar.events"
    );
    
    var service = new CalendarService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential
    });

    return service;
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddOpenIdConnect(options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Authority = "https://temasek-auth.from.sg";
        options.ClientId = "calendarr";

        options.ResponseType = OpenIdConnectResponseType.Code;

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "roles";
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.ConfigureCookieOidc(
    CookieAuthenticationDefaults.AuthenticationScheme,
    OpenIdConnectDefaults.AuthenticationScheme
);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<IncrementalSyncBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Temasek.Calendarr.Client._Imports).Assembly);

var auth = app.MapGroup("auth");

auth.MapGet("/login", () => TypedResults.Challenge())
    .AllowAnonymous();

auth.MapPost("/logout",
    () => TypedResults.SignOut(
        authenticationSchemes:
        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]
    )
);

app.MapSync();

app.Run();