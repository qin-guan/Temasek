using FastEndpoints;
using SqlSugar;

namespace Temasek.Operatorr.Features.ShiftRecord.List;

public class Endpoint(ISqlSugarClient db) : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/lol");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.StringAsync("NICE");
        // var re= await db.Queryable<SpecShiftRecord>().OrderBy(r => r.Start, OrderByType.Desc).FirstAsync(ct);
        //
        // await Send.OkAsync(new Response
        // {
        //     DcId = dc.UserId,
        //     DcName = dc.UserName,
        //     CdooId = cdoo.UserId,
        //     CdooName = cdoo.UserName,
        //     CdosId = cdos.UserId,
        //     CdosName = cdos.UserName,
        // }, ct);
    }
}