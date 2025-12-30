using System.Text;
using FastEndpoints;
using FastEndpoints.ClientGen.Kiota;
using FastEndpoints.Swagger;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Kiota.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using SqlSugar;
using Temasek.Clerk;
using Temasek.Clerk.Options;
using Temasek.Facilities.Options;
using Temasek.Facilities.Workers;
using Temasek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<AppOptions>().Bind(builder.Configuration.GetSection("App"));
builder.Services.AddOptions<ClerkOptions>().Bind(builder.Configuration.GetSection("Clerk"));
builder.Services.AddOptions<GoogleOptions>().Bind(builder.Configuration.GetSection("Google"));

builder.Services.SwaggerDocument(options =>
{
    options.DocumentSettings = s =>
    {
        s.DocumentName = "v1";
    };
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                builder.Environment.IsDevelopment()
                    ? "http://localhost:3003"
                    : "https://temasek-facilities.from.sg"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder
    .Services.AddFastEndpoints()
    .AddAuthorization()
    .AddAuthentication(ClerkAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ClerkAuthenticationHandler>(
        ClerkAuthenticationHandler.SchemeName,
        null
    );

builder.Services.AddSingleton<ISqlSugarClient>(s =>
{
    var sqlSugar = new SqlSugarScope(
        new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=app.db",
            IsAutoCloseConnection = true,
        },
        db =>
        {
            db.Aop.OnLogExecuting = (sql, p) =>
            {
                Console.WriteLine(sql);
            };
        }
    );

    return sqlSugar;
});

builder.Services.AddKeyedSingleton(
    "bookings",
    (sp, _) =>
    {
        var options = sp.GetRequiredService<IOptions<GoogleOptions>>();
        var s = Convert.FromBase64String(options.Value.ServiceAccountJsonCredential);

        var credential = CredentialFactory.FromJson<ServiceAccountCredential>(
            Encoding.UTF8.GetString(s)
        );
        credential.Scopes =
        [
            "https://www.googleapis.com/auth/calendar",
            "https://www.googleapis.com/auth/calendar.events",
        ];

        var service = new CalendarService(
            new BaseClientService.Initializer { HttpClientInitializer = credential }
        );

        return service;
    }
);

builder.Services.AddHostedService<BookingV1MigrationToV2BackgroundService>();

var app = builder.Build();

app.UseSwaggerGen(options =>
{
    options.Path = "/openapi/{documentName}.json";
});

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

await app.GenerateApiClientsAndExitAsync(c =>
{
    c.SwaggerDocumentName = "v1";
    c.Language = GenerationLanguage.TypeScript;
    c.OutputPath = Path.Combine(
        app.Environment.ContentRootPath,
        "..",
        "Temasek.Facilities.Client",
        "app",
        "api-client"
    );
    c.ClientNamespaceName = "TemasekFacilities";
    c.ClientClassName = "ApiClient";
});

app.MapDefaultEndpoints();
app.MapScalarApiReference();

app.Run();
