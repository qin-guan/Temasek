using SqlSugar;

namespace Temasek.Operatorr.Entities;

public abstract class ShiftRecord
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }
    public DateTimeOffset Start { get; set; }
    public string UserId { get; set; }
}