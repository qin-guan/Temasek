# Quick Start Guide: Google Calendar Sync Worker

This guide will help you quickly set up the Temasek Calendar Sync Worker to keep two Google Calendars synchronized in real-time.

## Prerequisites

1. **Google Cloud Account**: You need access to Google Cloud Console
2. **Two Google Calendars**: The source and target calendars to sync
3. **.NET 8 Runtime**: For running the worker service
4. **Docker** (optional): For containerized deployment

## Step 1: Set Up Google Cloud Service Account

### 1.1 Create a Google Cloud Project
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Note your Project ID

### 1.2 Enable Google Calendar API
1. Go to **APIs & Services** > **Library**
2. Search for "Google Calendar API"
3. Click **Enable**

### 1.3 Create Service Account
1. Go to **APIs & Services** > **Credentials**
2. Click **Create Credentials** > **Service Account**
3. Fill in the service account details:
   - **Name**: `calendar-sync-worker`
   - **Description**: `Service account for Temasek Calendar Sync Worker`
4. Click **Create and Continue**
5. Skip the optional steps and click **Done**

### 1.4 Generate Service Account Key
1. In the **Credentials** page, click on your service account
2. Go to the **Keys** tab
3. Click **Add Key** > **Create new key**
4. Choose **JSON** format
5. Click **Create** - this downloads the JSON key file
6. **Important**: Keep this file secure and never commit it to version control

### 1.5 Encode the Key File
Convert the JSON key to Base64 encoding:

**Linux/macOS:**
```bash
base64 -w 0 path/to/your/service-account-key.json
```

**Windows (PowerShell):**
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("path\to\your\service-account-key.json"))
```

Copy the resulting Base64 string - you'll need it for configuration.

## Step 2: Share Calendars with Service Account

1. Open **Google Calendar** in your browser
2. For **both** calendars (source and target):
   - Click the three dots next to the calendar name
   - Select **Settings and sharing**
   - Scroll to **Share with specific people**
   - Click **Add people**
   - Enter your service account email (found in the JSON key file, field `client_email`)
   - Set permission to **Make changes to events**
   - Click **Send**

## Step 3: Configure the Worker

### 3.1 Copy Environment Template
```bash
cp .env.example .env
```

### 3.2 Update Configuration
Edit `.env` file with your values:

```bash
# Your Base64-encoded service account JSON from Step 1.5
CALENDAR_SERVICE_ACCOUNT_JSON=eyJ0eXBlIjoic2VydmljZV9hY2NvdW50...

# Your source calendar (email or calendar ID)
PRIMARY_CALENDAR_ID=your-source-calendar@gmail.com

# Your target calendar (email or calendar ID)  
SECONDARY_CALENDAR_ID=your-backup-calendar@gmail.com

# Sync every 60 seconds (optional, defaults to 60000)
SYNC_INTERVAL_MS=60000
```

### 3.3 Find Calendar IDs (if needed)
If you need the calendar ID instead of email:
1. Open **Google Calendar**
2. Click the three dots next to your calendar
3. Select **Settings and sharing**
4. Scroll to **Integrate calendar**
5. Copy the **Calendar ID**

## Step 4: Run the Worker

### Option A: Run with .NET CLI
```bash
cd Temasek.CalendarSyncWorker
dotnet run
```

### Option B: Run with Docker Compose
```bash
# From the root directory
docker-compose up temasek.calendar-sync-worker
```

### Option C: Run as Docker Container
```bash
cd Temasek.CalendarSyncWorker
docker build -t calendar-sync-worker .
docker run --env-file ../.env calendar-sync-worker
```

## Step 5: Verify It's Working

You should see logs like:
```
info: Program[0]
      ✓ Service account credentials format is valid
info: Program[0]  
      ✓ Primary calendar ID is set: source@gmail.com
info: Program[0]
      ✓ Secondary calendar ID is set: backup@gmail.com
info: Program[0]
      Configuration validation passed!
info: Program[0]
      Starting Temasek Calendar Sync Worker
info: Temasek.CalendarSyncWorker.Worker[0]
      Calendar Sync Worker started. Sync interval: 60000ms
info: Temasek.CalendarSyncWorker.Worker[0]
      Starting calendar synchronization cycle
```

## Testing the Sync

1. **Create a test event** in your source calendar
2. **Wait up to 2 minutes** (one sync cycle + processing time)
3. **Check your target calendar** - the event should appear
4. **Modify the event** in the source calendar
5. **Wait again** - changes should sync to target calendar
6. **Delete the event** from source - it should be removed from target

## Troubleshooting

### Configuration Validation Errors
If you see validation errors on startup:
- **"ServiceAccountJsonCredential must be replaced"**: Update your `.env` file with the Base64-encoded JSON
- **"PrimaryCalendarId must be replaced"**: Set your actual source calendar ID or email
- **"SecondaryCalendarId must be replaced"**: Set your actual target calendar ID or email

### Authentication Errors
- **"The caller does not have permission"**: Make sure you shared both calendars with the service account email
- **"Invalid credentials"**: Verify your Base64 encoding is correct

### Sync Issues
- **Events not syncing**: Check that both calendars are shared with the service account
- **"429 Rate limit exceeded"**: Increase `SYNC_INTERVAL_MS` to reduce API calls
- **Only some events sync**: The worker syncs all events by default, check for event-specific errors in logs

### Getting Help
- Check the logs for detailed error messages
- Verify calendar sharing permissions in Google Calendar
- Ensure your service account has Calendar API access enabled
- Test your configuration by running `dotnet run` and checking the validation output

## Production Deployment

For production use:
1. **Use environment variables** instead of the `.env` file
2. **Store credentials securely** (Azure Key Vault, AWS Secrets Manager, etc.)
3. **Set up monitoring** using the built-in health check endpoints
4. **Configure log aggregation** for centralized logging
5. **Run as a systemd service** on Linux or Windows Service
6. **Set appropriate resource limits** for memory and CPU

The worker is designed to be robust and will automatically recover from temporary network issues or API errors.