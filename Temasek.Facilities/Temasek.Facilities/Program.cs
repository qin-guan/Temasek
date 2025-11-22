using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Temasek.Clerk;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Environment.IsDevelopment() ? "http://localhost:3003" : "https://temasek-facilities.from.sg")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services
    .AddFastEndpoints()
    .AddAuthorization()
    .AddAuthentication(ClerkAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ClerkAuthenticationHandler>(ClerkAuthenticationHandler.SchemeName, null);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();
