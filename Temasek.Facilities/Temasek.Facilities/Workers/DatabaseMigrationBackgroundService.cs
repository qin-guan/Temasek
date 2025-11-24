using SqlSugar;
using Temasek.Clerk;
using Temasek.Facilities.Models;

namespace Temasek.Facilities.Workers;

public class DatabaseMigrationBackgroundService(ISqlSugarClient db) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        db.DbMaintenance.CreateDatabase();

        db.CodeFirst.InitTables<Facility>();
        db.CodeFirst.InitTables<Booking>();

        if (await db.Queryable<Facility>().CountAsync() > 0)
            return;

        var eiger = await db.Insertable(
                new Facility
                {
                    Name = "Eiger",
                    Group = FacilityGroup.Outdoor,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var temasekSquare = await db.Insertable(
                new Facility
                {
                    Name = "Temasek Square",
                    Group = FacilityGroup.Outdoor,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var ballinger = await db.Insertable(
                new Facility
                {
                    Name = "Ballinger",
                    Group = FacilityGroup.Outdoor,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var mph = await db.Insertable(
                new Facility
                {
                    Name = "MPH",
                    Group = FacilityGroup.Indoor,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var mess = await db.Insertable(
                new Facility
                {
                    Name = "Mess",
                    Group = FacilityGroup.Indoor,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var futsalA = await db.Insertable(
                new Facility
                {
                    Name = "Futsal A",
                    Group = FacilityGroup.Futsal,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var futsalB = await db.Insertable(
                new Facility
                {
                    Name = "Futsal B",
                    Group = FacilityGroup.Futsal,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var audit = await db.Insertable(
                new Facility
                {
                    Name = "Audit",
                    Group = FacilityGroup.Conference,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var conferenceRoom = await db.Insertable(
                new Facility
                {
                    Name = "Conference Room",
                    Group = FacilityGroup.Conference,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();

        var kingon = await db.Insertable(
                new Facility
                {
                    Name = "Kingon",
                    Group = FacilityGroup.Others,
                    Scope = Scope.All,
                }
            )
            .ExecuteReturnEntityAsync();
    }
}
