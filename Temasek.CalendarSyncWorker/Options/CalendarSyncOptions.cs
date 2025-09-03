namespace Temasek.CalendarSyncWorker.Options;

public class CalendarSyncOptions
{
    /// <summary>
    /// Base64-encoded JSON service account credentials for Google Calendar API
    /// </summary>
    public required string ServiceAccountJsonCredential { get; set; }
    
    /// <summary>
    /// ID of the primary (source) calendar to sync from
    /// </summary>
    public required string PrimaryCalendarId { get; set; }
    
    /// <summary>
    /// ID of the secondary (target) calendar to sync to
    /// </summary>
    public required string SecondaryCalendarId { get; set; }
    
    /// <summary>
    /// Sync interval in milliseconds (default: 60000 = 1 minute)
    /// </summary>
    public int SyncIntervalMs { get; set; } = 60000;
    
    /// <summary>
    /// Maximum retry attempts for failed sync operations (default: 3)
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Initial retry delay in milliseconds (default: 5000 = 5 seconds)
    /// </summary>
    public int InitialRetryDelayMs { get; set; } = 5000;
    
    /// <summary>
    /// Maximum number of events to process in a single sync batch (default: 100)
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;
    
    /// <summary>
    /// Whether to perform a full sync on startup (default: true)
    /// </summary>
    public bool PerformFullSyncOnStartup { get; set; } = true;
}