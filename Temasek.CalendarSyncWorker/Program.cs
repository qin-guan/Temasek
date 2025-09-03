using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using Temasek.CalendarSyncWorker;
using Temasek.CalendarSyncWorker.Options;
using Temasek.CalendarSyncWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure calendar sync options
builder.Services.Configure<CalendarSyncOptions>(
    builder.Configuration.GetSection("CalendarSync"));

// Add Google Calendar service
builder.Services.AddSingleton<CalendarService>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<CalendarSyncOptions>>().Value;
    
    // Decode the base64 service account credentials
    var credentialBytes = Convert.FromBase64String(options.ServiceAccountJsonCredential);
    var credentialJson = Encoding.UTF8.GetString(credentialBytes);
    
    // Create the Google credential with required scopes
    var credential = GoogleCredential.FromJson(credentialJson).CreateScoped(
        CalendarService.Scope.Calendar,
        CalendarService.Scope.CalendarEvents
    );
    
    // Initialize the Calendar service
    var service = new CalendarService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential,
        ApplicationName = "Temasek Calendar Sync Worker"
    });
    
    return service;
});

// Add sync service and health check service
builder.Services.AddSingleton<CalendarSyncService>();
builder.Services.AddSingleton<HealthCheckService>();
builder.Services.AddSingleton<ConfigurationValidationService>();

// Add the hosted worker service
builder.Services.AddHostedService<Worker>();

// Configure logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var host = builder.Build();

// Get required services for startup validation and logging
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var options = host.Services.GetRequiredService<IOptions<CalendarSyncOptions>>().Value;

// Validate configuration on startup
var validationService = host.Services.GetRequiredService<ConfigurationValidationService>();
var validationResult = validationService.ValidateConfiguration();
validationResult.LogResults(logger);

if (!validationResult.IsValid)
{
    logger.LogCritical("Configuration validation failed. Please fix the configuration errors before running the worker.");
    Environment.ExitCode = 1;
    return;
}

// Log startup information
logger.LogInformation("Starting Temasek Calendar Sync Worker");
logger.LogInformation("Primary Calendar: {PrimaryId}", options.PrimaryCalendarId);
logger.LogInformation("Secondary Calendar: {SecondaryId}", options.SecondaryCalendarId);
logger.LogInformation("Sync Interval: {IntervalMs}ms", options.SyncIntervalMs);

await host.RunAsync();
