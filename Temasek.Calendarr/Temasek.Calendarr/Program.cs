using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using Temasek.Calendarr.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

builder.Services.AddKeyedSingleton<CalendarService>("BdeComd", (sp, _) =>
{
    var options = sp.GetRequiredService<IOptions<SyncOptions>>();
    var s = Convert.FromBase64String(options.Value.BdeComdServiceAccountJsonCredential);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
