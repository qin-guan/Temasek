namespace Temasek.Clerk.Options;

public class ClerkOptions
{
    public required string SecretKey { get; set; }
    public required string[] AuthorizedParties { get; set; } 
}
