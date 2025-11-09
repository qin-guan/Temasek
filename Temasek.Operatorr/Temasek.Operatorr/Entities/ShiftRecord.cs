using SqlSugar;

namespace Temasek.Operatorr.Entities;

public class ShiftRecord
{
    [SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; }
    public ShiftRecordType Type { get; set; }
    public DateTimeOffset Start { get; set; }
    public string UserId { get; set; }
    public string UserPhone { get; set; }
    public string UserName { get; set; }
}