namespace Temasek.Auth.Data;

public class SessionAuthentication
{
    public Guid Id { get; set; }
    public string Qs { get; set; }
    
    public string? Name { get; set; }
    public string? Nric { get; set; }
}