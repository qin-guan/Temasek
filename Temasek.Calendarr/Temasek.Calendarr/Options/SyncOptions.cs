namespace Temasek.Calendarr.Options;

public class SyncOptions
{
    public required string ServiceAccountJsonCredential { get; set; }
    public required string PrimaryCalendarId { get; set; }
    public required string SecondaryCalendarId { get; set; }
}