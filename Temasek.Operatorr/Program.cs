using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SqlSugar;
using Temasek.Operatorr.Components;
using Temasek.Operatorr.Extensions;
using Temasek.Operatorr.Workers;

var builder = WebApplication.CreateBuilder(args);

#region Auth

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddOpenIdConnect(options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Authority = "https://temasek-auth.from.sg";
        options.ClientId = "operatorr";
        
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.TokenValidationParameters.NameClaimType = "given_name";
        options.TokenValidationParameters.RoleClaimType = "roles";
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.ConfigureCookieOidc(
    CookieAuthenticationDefaults.AuthenticationScheme,
    OpenIdConnectDefaults.AuthenticationScheme
);

#endregion

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddBootstrapBlazor();

builder.Services.AddScoped<ISqlSugarClient>(s =>
{
    var sqlSugar = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=app.db",
            IsAutoCloseConnection = true,
            LanguageType = LanguageType.English
        },
        db =>
        {
            db.Aop.OnLogExecuting = (s1, parameters) =>
            {
                Console.WriteLine(s1);
            };
        });
    return sqlSugar;
});

builder.Services.AddHostedService<DatabaseBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var auth = app.MapGroup("auth");

auth.MapGet("/login", () => TypedResults.Challenge())
    .AllowAnonymous();

auth.MapPost("/logout",
    () => TypedResults.SignOut(
        authenticationSchemes:
        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]
    )
);

app.Run();