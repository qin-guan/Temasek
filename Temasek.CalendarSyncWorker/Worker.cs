using Microsoft.Extensions.Options;
using Temasek.CalendarSyncWorker.Options;
using Temasek.CalendarSyncWorker.Services;

namespace Temasek.CalendarSyncWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly CalendarSyncService _syncService;
    private readonly HealthCheckService _healthCheckService;
    private readonly CalendarSyncOptions _options;

    public Worker(
        ILogger<Worker> logger, 
        CalendarSyncService syncService,
        HealthCheckService healthCheckService,
        IOptions<CalendarSyncOptions> options)
    {
        _logger = logger;
        _syncService = syncService;
        _healthCheckService = healthCheckService;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Calendar Sync Worker started. Sync interval: {Interval}ms", _options.SyncIntervalMs);

        // Initial delay to ensure services are fully initialized
        await Task.Delay(5000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var syncStartTime = DateTime.UtcNow;
                _logger.LogInformation("Starting calendar synchronization cycle at {Time}", syncStartTime);

                await _syncService.SyncCalendarsAsync(stoppingToken);

                var syncDuration = DateTime.UtcNow - syncStartTime;
                _logger.LogInformation("Calendar synchronization completed successfully in {Duration}ms", 
                    syncDuration.TotalMilliseconds);

                // Record successful sync for health monitoring
                _healthCheckService.RecordSyncResult(true);

                // Wait for the configured interval before next sync
                await Task.Delay(_options.SyncIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Calendar sync worker is shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during calendar synchronization cycle");
                
                // Record failed sync for health monitoring
                _healthCheckService.RecordSyncResult(false);
                
                // Wait before retrying (shorter interval on error)
                var errorRetryDelay = Math.Min(_options.SyncIntervalMs, _options.InitialRetryDelayMs);
                _logger.LogInformation("Retrying in {Delay}ms due to error", errorRetryDelay);
                
                try
                {
                    await Task.Delay(errorRetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("Calendar Sync Worker stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calendar Sync Worker stop requested");
        await base.StopAsync(cancellationToken);
    }
}
