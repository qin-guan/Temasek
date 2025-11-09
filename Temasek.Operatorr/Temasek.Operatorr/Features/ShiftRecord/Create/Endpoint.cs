using System.Security.Claims;
using FastEndpoints;
using SqlSugar;

namespace Temasek.Operatorr.Features.ShiftRecord.Create;

public class Endpoint(ISqlSugarClient db) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/ShiftRecord");
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        var shift = await db.Queryable<Entities.ShiftRecord>()
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Start, OrderByType.Desc)
            .FirstAsync(ct);

        if (shift is not null && shift.UserId == userId)
        {
            ThrowError("User is already on duty.");
            return;
        }

        shift = await db.Insertable(new Entities.ShiftRecord
            {
                Start = DateTimeOffset.UtcNow,
                UserPhone = req.UserPhone,
                UserName = req.UserName,
                UserId = userId
            })
            .ExecuteReturnEntityAsync();

        await Send.OkAsync(new Response
        {
            Id = shift.Id,
            Start = shift.Start,
            UserId = shift.UserId,
            UserPhone = shift.UserPhone,
            UserName = shift.UserName
        }, ct);
    }
}