namespace Temasek.Auth.Options;

public class FormSgOptions
{
    public required string CallbackApiKey { get; set; }
    /// <summary>
    /// Secret key to for JWT sent to the prefilled form field
    /// </summary>
    public required string SecretKey { get; set; }
    public required string FormId { get; set; }
    public required string PrefillFieldId { get; set; }
}
