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
        
        if (!await client.Queryable<ClerkShiftRecord>().AnyAsync(stoppingToken))
        {
            await client.Insertable(new ClerkShiftRecord
            {
                Start = DateTimeOffset.Now,
                UserId = "XXX",
                Phone = "8888 8888"
            }).ExecuteCommandAsync(stoppingToken);
        }
        
        if (!await client.Queryable<OfficerShiftRecord>().AnyAsync(stoppingToken))
        {
            await client.Insertable(new OfficerShiftRecord()
            {
                Start = DateTimeOffset.Now,
                UserId = "XXX",
                Phone = "8888 8888"
            }).ExecuteCommandAsync(stoppingToken);
        }
        
        if (!await client.Queryable<SpecShiftRecord>().AnyAsync(stoppingToken))
        {
            await client.Insertable(new SpecShiftRecord()
            {
                Start = DateTimeOffset.Now,
                UserId = "XXX",
                Phone = "8888 8888"
            }).ExecuteCommandAsync(stoppingToken);
        }
    }
}