using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Temasek.CalendarSyncWorker.Options;

namespace Temasek.CalendarSyncWorker.Services;

/// <summary>
/// Health check service for monitoring the calendar sync worker status
/// </summary>
public class HealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly CalendarSyncOptions _options;
    private DateTime _lastSuccessfulSync = DateTime.MinValue;
    private int _consecutiveFailures = 0;

    public HealthCheckService(ILogger<HealthCheckService> logger, IOptions<CalendarSyncOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Updates the health status after a sync operation
    /// </summary>
    public void RecordSyncResult(bool success)
    {
        if (success)
        {
            _lastSuccessfulSync = DateTime.UtcNow;
            _consecutiveFailures = 0;
            _logger.LogDebug("Health check: Sync successful at {Time}", _lastSuccessfulSync);
        }
        else
        {
            _consecutiveFailures++;
            _logger.LogWarning("Health check: Sync failed, consecutive failures: {Count}", _consecutiveFailures);
        }
    }

    /// <summary>
    /// Gets the current health status
    /// </summary>
    public HealthStatus GetHealthStatus()
    {
        var timeSinceLastSuccess = DateTime.UtcNow - _lastSuccessfulSync;
        var maxAllowedAge = TimeSpan.FromMilliseconds(_options.SyncIntervalMs * 3); // Allow 3 missed cycles

        if (_lastSuccessfulSync == DateTime.MinValue)
        {
            // Never synced successfully
            if (DateTime.UtcNow.Subtract(DateTime.MinValue) > TimeSpan.FromMinutes(10))
            {
                return HealthStatus.Unhealthy;
            }
            return HealthStatus.Starting;
        }

        if (_consecutiveFailures >= _options.MaxRetryAttempts)
        {
            return HealthStatus.Unhealthy;
        }

        if (timeSinceLastSuccess > maxAllowedAge)
        {
            return HealthStatus.Degraded;
        }

        return HealthStatus.Healthy;
    }

    /// <summary>
    /// Gets detailed health information
    /// </summary>
    public HealthInfo GetHealthInfo()
    {
        var status = GetHealthStatus();
        var timeSinceLastSuccess = _lastSuccessfulSync == DateTime.MinValue 
            ? TimeSpan.Zero 
            : DateTime.UtcNow - _lastSuccessfulSync;

        return new HealthInfo
        {
            Status = status,
            LastSuccessfulSync = _lastSuccessfulSync,
            TimeSinceLastSuccess = timeSinceLastSuccess,
            ConsecutiveFailures = _consecutiveFailures,
            Message = GetHealthMessage(status)
        };
    }

    private string GetHealthMessage(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Healthy => "Calendar sync is operating normally",
            HealthStatus.Degraded => $"Calendar sync is behind schedule. Last success: {_lastSuccessfulSync:yyyy-MM-dd HH:mm:ss}",
            HealthStatus.Unhealthy => $"Calendar sync is failing. Consecutive failures: {_consecutiveFailures}",
            HealthStatus.Starting => "Calendar sync worker is starting up",
            _ => "Unknown health status"
        };
    }
}

public enum HealthStatus
{
    Starting,
    Healthy,
    Degraded,
    Unhealthy
}

public class HealthInfo
{
    public HealthStatus Status { get; set; }
    public DateTime LastSuccessfulSync { get; set; }
    public TimeSpan TimeSinceLastSuccess { get; set; }
    public int ConsecutiveFailures { get; set; }
    public string Message { get; set; } = string.Empty;
}