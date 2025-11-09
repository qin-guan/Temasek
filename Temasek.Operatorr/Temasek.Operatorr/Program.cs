using Dm.util;
using FastEndpoints;
using FastEndpoints.ClientGen.Kiota;
using FastEndpoints.Swagger;
using Kiota.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SqlSugar;
using Temasek.Operatorr.Entities;
using Temasek.Operatorr.Extensions;
using Temasek.Operatorr.Workers;
using Temasek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o => { o.DocumentSettings = s => s.DocumentName = "v1"; });

builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<ISqlSugarClient>((sp) =>
{
    var sqlSugar = new SqlSugarScope(new ConnectionConfig
    {
        DbType = DbType.Sqlite,
        ConnectionString = "DataSource=app.db",
        IsAutoCloseConnection = true,
    });
    
    return sqlSugar;
});

#region Auth

builder.Services
    .AddAuthentication(options =>
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

builder.Services.AddAuthorization();

#endregion

builder.Services.AddHostedService<DatabaseBackgroundService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

await app.GenerateApiClientsAndExitAsync(c =>
{
    c.SwaggerDocumentName = "v1";
    c.Language = GenerationLanguage.TypeScript;
    c.OutputPath = Path.Join(app.Environment.ContentRootPath, "..", "Temasek.Operatorr.Client", "api");
    c.ClientNamespaceName = "Operatorr";
    c.ClientClassName = "ApiClient";
});

app.Run();