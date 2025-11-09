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
        
        client.CodeFirst.InitTables<ShiftRecord>();

        if (!await client.Queryable<ShiftRecord>().AnyAsync(stoppingToken))
        {
            await client.Insertable(new ShiftRecord
            {
                Start = DateTimeOffset.UtcNow,
                UserName = "Qin Guan",
                UserId = "XXX",
                UserPhone = "8888 8888"
            }).ExecuteCommandAsync(stoppingToken);
            
            await client.Insertable(new ShiftRecord
            {
                Start = DateTimeOffset.UtcNow,
                UserName = "Qin Guan",
                UserId = "XXX",
                UserPhone = "8888 8888"
            }).ExecuteCommandAsync(stoppingToken);
            
            await client.Insertable(new ShiftRecord
            {
                Start = DateTimeOffset.UtcNow,
                UserName = "Qin Guan",
                UserId = "XXX",
                UserPhone = "8888 8888"
            }).ExecuteCommandAsync(stoppingToken);
        }
    }
}