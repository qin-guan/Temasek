using System.Collections.Concurrent;

namespace Temasek.Calendarr.Features.Sync;

public class InMemoryLogService
{
    public event Action? OnLogsChanged;

    private readonly ConcurrentQueue<LogMessage> _logs = new();

    public void AddLog(LogLevel level, string category, string message)
    {
        _logs.Enqueue(new LogMessage(DateTime.Now, level, category, message));

        while (_logs.Count > 1000 && _logs.TryDequeue(out _))
        {
        }

        OnLogsChanged?.Invoke();
    }

    public IReadOnlyList<LogMessage> GetLogs()
    {
        return [.._logs];
    }

    public record LogMessage(DateTime Timestamp, LogLevel Level, string? Category, string? Message);
}