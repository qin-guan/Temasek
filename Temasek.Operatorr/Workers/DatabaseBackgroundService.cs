using SqlSugar;
using Temasek.Operatorr.Entities;

namespace Temasek.Operatorr.Workers;

public class DatabaseBackgroundService(IServiceProvider sp) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = sp.CreateAsyncScope();
        var client = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        client.DbMaintenance.CreateDatabase();
        client.CodeFirst.InitTables<ClerkShiftRecord>();
        client.CodeFirst.InitTables<OfficerShiftRecord>();
        client.CodeFirst.InitTables<SpecShiftRecord>();
    }
}