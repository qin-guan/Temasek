using System.Text;
using Microsoft.Extensions.Options;
using Temasek.CalendarSyncWorker.Options;

namespace Temasek.CalendarSyncWorker.Services;

/// <summary>
/// Service to validate configuration and test connectivity
/// </summary>
public class ConfigurationValidationService
{
    private readonly ILogger<ConfigurationValidationService> _logger;
    private readonly CalendarSyncOptions _options;

    public ConfigurationValidationService(
        ILogger<ConfigurationValidationService> logger,
        IOptions<CalendarSyncOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Validates the configuration and returns validation results
    /// </summary>
    public ValidationResult ValidateConfiguration()
    {
        var result = new ValidationResult();

        _logger.LogInformation("Validating calendar sync configuration...");

        // Validate service account credentials
        if (string.IsNullOrWhiteSpace(_options.ServiceAccountJsonCredential))
        {
            result.AddError("ServiceAccountJsonCredential is required");
        }
        else if (_options.ServiceAccountJsonCredential == "BASE64_ENCODED_SERVICE_ACCOUNT_JSON")
        {
            result.AddError("ServiceAccountJsonCredential must be replaced with actual Base64-encoded JSON");
        }
        else
        {
            try
            {
                var credentialBytes = Convert.FromBase64String(_options.ServiceAccountJsonCredential);
                var credentialJson = Encoding.UTF8.GetString(credentialBytes);
                
                if (!credentialJson.Contains("private_key") || !credentialJson.Contains("client_email"))
                {
                    result.AddError("ServiceAccountJsonCredential does not appear to be a valid service account JSON");
                }
                else
                {
                    result.AddSuccess("Service account credentials format is valid");
                }
            }
            catch (Exception ex)
            {
                result.AddError($"Invalid Base64 encoding for ServiceAccountJsonCredential: {ex.Message}");
            }
        }

        // Validate calendar IDs
        if (string.IsNullOrWhiteSpace(_options.PrimaryCalendarId))
        {
            result.AddError("PrimaryCalendarId is required");
        }
        else if (_options.PrimaryCalendarId.Contains("your-primary-calendar"))
        {
            result.AddError("PrimaryCalendarId must be replaced with actual calendar ID or email");
        }
        else
        {
            result.AddSuccess($"Primary calendar ID is set: {_options.PrimaryCalendarId}");
        }

        if (string.IsNullOrWhiteSpace(_options.SecondaryCalendarId))
        {
            result.AddError("SecondaryCalendarId is required");
        }
        else if (_options.SecondaryCalendarId.Contains("your-secondary-calendar"))
        {
            result.AddError("SecondaryCalendarId must be replaced with actual calendar ID or email");
        }
        else
        {
            result.AddSuccess($"Secondary calendar ID is set: {_options.SecondaryCalendarId}");
        }

        if (_options.PrimaryCalendarId == _options.SecondaryCalendarId && 
            !string.IsNullOrWhiteSpace(_options.PrimaryCalendarId))
        {
            result.AddWarning("Primary and secondary calendar IDs are the same - this may cause issues");
        }

        // Validate numeric settings
        if (_options.SyncIntervalMs < 10000)
        {
            result.AddWarning($"SyncIntervalMs ({_options.SyncIntervalMs}) is very short, may hit rate limits");
        }
        else if (_options.SyncIntervalMs > 3600000)
        {
            result.AddWarning($"SyncIntervalMs ({_options.SyncIntervalMs}) is very long (>1 hour), sync may not be timely");
        }
        else
        {
            result.AddSuccess($"Sync interval is reasonable: {_options.SyncIntervalMs}ms ({_options.SyncIntervalMs / 1000}s)");
        }

        if (_options.MaxBatchSize < 10 || _options.MaxBatchSize > 2500)
        {
            result.AddWarning($"MaxBatchSize ({_options.MaxBatchSize}) is outside recommended range (10-2500)");
        }

        if (_options.MaxRetryAttempts < 1 || _options.MaxRetryAttempts > 10)
        {
            result.AddWarning($"MaxRetryAttempts ({_options.MaxRetryAttempts}) is outside recommended range (1-10)");
        }

        _logger.LogInformation("Configuration validation completed. Errors: {ErrorCount}, Warnings: {WarningCount}, Success: {SuccessCount}",
            result.Errors.Count, result.Warnings.Count, result.Successes.Count);

        return result;
    }
}

public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> Successes { get; } = new();

    public bool IsValid => !Errors.Any();

    public void AddError(string message)
    {
        Errors.Add(message);
    }

    public void AddWarning(string message)
    {
        Warnings.Add(message);
    }

    public void AddSuccess(string message)
    {
        Successes.Add(message);
    }

    public void LogResults(ILogger logger)
    {
        foreach (var success in Successes)
        {
            logger.LogInformation("✓ {Message}", success);
        }

        foreach (var warning in Warnings)
        {
            logger.LogWarning("⚠ {Message}", warning);
        }

        foreach (var error in Errors)
        {
            logger.LogError("✗ {Message}", error);
        }

        if (IsValid)
        {
            logger.LogInformation("Configuration validation passed!");
        }
        else
        {
            logger.LogError("Configuration validation failed with {ErrorCount} errors", Errors.Count);
        }
    }
}