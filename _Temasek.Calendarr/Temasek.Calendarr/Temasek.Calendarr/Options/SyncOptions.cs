namespace Temasek.Calendarr.Options;

public class SyncOptions
{
    public required string ServiceAccountJsonCredential { get; set; }
    public required string BdeComdServiceAccountJsonCredential { get; set; }
    public required string ParentCalendarId { get; set; }
    public required string ChildCalendarId { get; set; }
    public required TimeSpan SyncInterval { get; set; }
}
