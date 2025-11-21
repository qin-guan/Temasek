using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using Polly;
using Temasek.Calendarr.Hubs;
using Temasek.Calendarr.Logger;
using Temasek.Calendarr.Options;
using Temasek.Calendarr.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<SyncOptions>()
    .Bind(builder.Configuration.GetSection("Sync"));
builder.Services.AddOptions<BdeComdOptions>()
    .Bind(builder.Configuration.GetSection("BdeComd"));

builder.Services.AddResiliencePipeline("BackgroundService", options =>
{
    options
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            Delay = TimeSpan.FromSeconds(10)
        })
        .AddTimeout(TimeSpan.FromMinutes(15));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:3002");
        }
        else
        {
            policy.WithOrigins("https://temasek-calendarr.from.sg");
        }
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSignalR();
builder.Services.AddOpenApi();

builder.Services.AddKeyedSingleton("Sync", (sp, _) =>
{
    var options = sp.GetRequiredService<IOptions<SyncOptions>>();
    var s = Convert.FromBase64String(options.Value.ServiceAccountJsonCredential);

    var credential = CredentialFactory.FromJson<ServiceAccountCredential>(Encoding.UTF8.GetString(s));
    credential.Scopes = [
        "https://www.googleapis.com/auth/calendar",
        "https://www.googleapis.com/auth/calendar.events"
    ];

    var service = new CalendarService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential
    });

    return service;
});

builder.Services.AddKeyedSingleton("BdeComd", (sp, _) =>
{
    var options = sp.GetRequiredService<IOptions<SyncOptions>>();
    var s = Convert.FromBase64String(options.Value.ServiceAccountJsonCredential);

    var credential = CredentialFactory.FromJson<ServiceAccountCredential>(Encoding.UTF8.GetString(s));
    credential.Scopes = [
        "https://www.googleapis.com/auth/calendar",
        "https://www.googleapis.com/auth/calendar.events"
    ];

    var service = new CalendarService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential
    });

    return service;
});

builder.Services.AddSingleton<ILoggerProvider, SignalRLoggerProvider>();

builder.Services.AddHostedService<BdeComdSourceConflictBackgroundService>();
builder.Services.AddHostedService<SyncIncrementalBackgroundService>();

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<LoggerHub>("/Hubs/Logger");

app.Run();
