using Microsoft.AspNetCore.SignalR;
using Temasek.Calendarr.Services;

namespace Temasek.Calendarr.Hubs;

public class SyncHub(
    CalendarSyncService syncService
) : Hub
{
    public async Task RunFullSyncAsync()
    {
        await syncService.RunFullSyncAsync();
    }
}
