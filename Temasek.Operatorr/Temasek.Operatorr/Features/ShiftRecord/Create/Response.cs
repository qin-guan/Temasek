namespace Temasek.Operatorr.Features.ShiftRecord.Create;

public class Response
{
    
    public required Guid Id { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required string UserId { get; init; }
    public required string UserPhone { get; init; }
    public required string UserName { get; init; }
}