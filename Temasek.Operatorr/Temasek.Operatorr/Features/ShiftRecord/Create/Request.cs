namespace Temasek.Operatorr.Features.ShiftRecord.Create;

public class Request
{
    /// <summary>
    /// Due to the "decentralized" nature of the authentication mechanism, we kind of just rely on the client to provide an accurate value of the users name.
    /// </summary>
    public required string UserName { get; init; }

    public required string UserPhone { get; init; }
}