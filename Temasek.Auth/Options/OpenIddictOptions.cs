namespace Temasek.Auth.Options;

public class OpenIddictOptions
{
    public required List<OpenIddictClient> Clients { get; set; } = [];
}

public class OpenIddictClient
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public HashSet<Uri> RedirectUris { get; set; } = [];
}