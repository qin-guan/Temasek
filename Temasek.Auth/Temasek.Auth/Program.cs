using Clerk.BackendAPI;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Temasek.Auth.Options;
using Temasek.Clerk;
using Temasek.Clerk.Options;
using Temasek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<FormSgOptions>()
    .Bind(builder.Configuration.GetSection("FormSg"));

builder.Services.AddOptions<ClerkOptions>()
    .Bind(builder.Configuration.GetSection("Clerk"));

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddFastEndpoints()
    .AddAuthorization()
    .AddAuthentication(ClerkAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ClerkAuthenticationHandler>(ClerkAuthenticationHandler.SchemeName, null);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Environment.IsDevelopment() ? "http://localhost:3000" : "https://temasek-auth.from.sg")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddOpenApi();

builder.Services.AddScoped(sp => new ClerkBackendApi(bearerAuth: sp.GetRequiredService<IOptions<ClerkOptions>>().Value.SecretKey));

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();
