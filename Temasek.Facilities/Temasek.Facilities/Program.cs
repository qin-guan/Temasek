using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using SqlSugar;
using Temasek.Clerk;
using Temasek.Facilities.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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

builder.Services.AddHostedService<DatabaseMigrationBackgroundService>();

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
