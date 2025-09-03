# Temasek Calendar Sync Worker

A .NET 8 background worker service that keeps two Google Calendars in sync in real-time with incremental updates.

## Features

- **Real-time Synchronization**: Continuously monitors and syncs calendar changes
- **Incremental Updates**: Uses Google Calendar sync tokens for efficient incremental syncing
- **Bi-directional Sync**: Keeps source and target calendars synchronized
- **Robust Error Handling**: Includes retry logic and graceful error recovery
- **Configurable**: Flexible configuration options for sync intervals, batch sizes, and more
- **Privacy-Aware**: Removes attendee information during sync for privacy

## Prerequisites

1. **Google Service Account**: You need a Google Cloud service account with Calendar API access
2. **Calendar Access**: The service account must have access to both calendars you want to sync
3. **.NET 8 Runtime**: Required to run the worker service

## Configuration

### Setting up Google Service Account

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google Calendar API
4. Create a service account and download the JSON key file
5. Share your Google Calendars with the service account email address
6. Convert the JSON key file to Base64 encoding

### Application Configuration

Update `appsettings.json` with your configuration:

```json
{
  "CalendarSync": {
    "ServiceAccountJsonCredential": "BASE64_ENCODED_SERVICE_ACCOUNT_JSON",
    "PrimaryCalendarId": "primary-calendar@gmail.com",
    "SecondaryCalendarId": "secondary-calendar@gmail.com",
    "SyncIntervalMs": 60000,
    "MaxRetryAttempts": 3,
    "InitialRetryDelayMs": 5000,
    "MaxBatchSize": 100,
    "PerformFullSyncOnStartup": true
  }
}
```

#### Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `ServiceAccountJsonCredential` | Base64-encoded Google service account JSON | Required |
| `PrimaryCalendarId` | Source calendar ID (email or ID) | Required |
| `SecondaryCalendarId` | Target calendar ID (email or ID) | Required |
| `SyncIntervalMs` | Sync interval in milliseconds | 60000 (1 minute) |
| `MaxRetryAttempts` | Maximum retry attempts for failed operations | 3 |
| `InitialRetryDelayMs` | Initial delay before retry in milliseconds | 5000 (5 seconds) |
| `MaxBatchSize` | Maximum events to process per batch | 100 |
| `PerformFullSyncOnStartup` | Whether to do full sync on startup | true |

## Running the Service

### Development

```bash
cd Temasek.CalendarSyncWorker
dotnet run
```

### Production

```bash
cd Temasek.CalendarSyncWorker
dotnet build -c Release
dotnet run -c Release
```

### As a Windows Service

```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Install as Windows Service (requires admin privileges)
sc create "Temasek Calendar Sync" binPath= "C:\path\to\publish\Temasek.CalendarSyncWorker.exe"
sc start "Temasek Calendar Sync"
```

### As a Linux Systemd Service

1. Create a service file at `/etc/systemd/system/calendar-sync.service`:

```ini
[Unit]
Description=Temasek Calendar Sync Worker
After=network.target

[Service]
Type=notify
ExecStart=/path/to/publish/Temasek.CalendarSyncWorker
Restart=always
User=calendar-sync
WorkingDirectory=/path/to/publish

[Install]
WantedBy=multi-user.target
```

2. Enable and start the service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable calendar-sync
sudo systemctl start calendar-sync
```

## How It Works

1. **Initialization**: On startup, the worker performs a full synchronization if configured
2. **Incremental Sync**: Subsequent syncs use Google Calendar's sync tokens for efficient incremental updates
3. **Event Processing**: 
   - New events in the primary calendar are created in the secondary calendar
   - Updated events are synchronized with their changes
   - Deleted events are removed from the secondary calendar
   - Orphaned events (existing in secondary but not primary) are cleaned up
4. **Error Recovery**: If incremental sync fails, the worker falls back to full sync
5. **Rate Limiting**: Concurrent operations are limited to avoid API rate limits

## Monitoring and Logs

The service provides detailed logging:

- **Information**: Sync cycles, event counts, and completion status
- **Debug**: Individual event operations (create, update, delete)
- **Error**: Failed operations with full exception details
- **Warning**: Non-critical issues like missing sync tokens

Log levels can be configured in `appsettings.json`.

## Limitations

- **One-way Sync**: Currently syncs from primary to secondary calendar only
- **Attendee Privacy**: Attendee information is removed during sync
- **API Limits**: Subject to Google Calendar API rate limits
- **Network Dependency**: Requires stable internet connection

## Troubleshooting

### Common Issues

1. **Authentication Errors**:
   - Verify service account JSON is correctly Base64 encoded
   - Ensure service account has Calendar API access
   - Check that calendars are shared with the service account

2. **Sync Token Issues**:
   - If incremental sync fails repeatedly, the worker will reset and perform full sync
   - Sync tokens expire after some time of inactivity

3. **Rate Limiting**:
   - The worker includes built-in rate limiting
   - If you hit limits, increase the `SyncIntervalMs` value

4. **Memory Usage**:
   - For large calendars, consider reducing `MaxBatchSize`
   - Monitor memory usage in production

## Security Considerations

- Store service account credentials securely
- Use environment variables or Azure Key Vault for production deployments
- Regularly rotate service account keys
- Monitor service account usage in Google Cloud Console

## License

This project is part of the Temasek suite and follows the same licensing terms.