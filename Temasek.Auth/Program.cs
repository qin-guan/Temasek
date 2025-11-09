using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Scalar.AspNetCore;
using Temasek.Auth.Data;
using Temasek.Auth.Features;
using Temasek.Auth.Options;
using Temasek.Auth.Workers;
using Temasek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<FormGovSgOptions>()
    .Bind(builder.Configuration.GetSection("FormGovSg"));

builder.Services.AddOptions<OpenIddictOptions>()
    .Bind(builder.Configuration.GetSection("OpenIddict"));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddCors();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(5); });
builder.Services.AddOpenApi();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddHostedService<OpenIddictBackgroundService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
    options.UseOpenIddict();
});

builder.Services
    .AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetEndSessionEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token")
            .SetUserInfoEndpointUris("connect/userinfo");

        options.RegisterScopes(
            OpenIddictConstants.Scopes.OfflineAccess,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles
        );

        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        options.RequireProofKeyForCodeExchange();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await using var scope = app.Services.CreateAsyncScope();
var openIddictOptions = scope.ServiceProvider.GetRequiredService<IOptions<OpenIddictOptions>>();

app.UseCors(p =>
{
    foreach (var uri in openIddictOptions.Value.Clients.SelectMany(c => c.RedirectUris))
    {
        p.WithOrigins(uri.Scheme + "://" + uri.Host).AllowAnyHeader().AllowCredentials();
    }
});
app.UseSession();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapOidcEndpoints();

app.Run();