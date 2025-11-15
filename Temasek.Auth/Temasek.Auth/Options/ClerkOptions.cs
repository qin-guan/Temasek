namespace Temasek.Auth.Options;

public class ClerkOptions
{
    public required string SecretKey { get; set; }
    public required string[] AuthorizedParties { get; set; } 
}
