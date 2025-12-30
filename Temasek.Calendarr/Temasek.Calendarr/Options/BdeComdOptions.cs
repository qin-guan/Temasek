namespace Temasek.Calendarr.Options;

public class BdeComdOptions
{
    /// <summary>
    /// Source calendar to monitor for new events
    /// </summary>
    public required string SourceCalendarId { get; set; }

    /// <summary>
    /// Target calendar to compare schedule against source calendar
    /// </summary>
    public required string TargetCalendarId { get; set; }
    public required TimeSpan SyncInterval { get; set; }
    public string? VocabPath { get; set; }
    public string? ModelPath { get; set; }
}
