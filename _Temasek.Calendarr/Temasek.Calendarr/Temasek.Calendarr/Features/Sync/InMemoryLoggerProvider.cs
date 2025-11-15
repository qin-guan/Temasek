namespace Temasek.Calendarr.Features.Sync;

public class InMemoryLoggerProvider(InMemoryLogService logService) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new InMemoryLogger(categoryName, logService);
    }

    public void Dispose()
    {
    }

    private class InMemoryLogger(string category, InMemoryLogService logService) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            var message = formatter(state, exception);
            
            if (exception is not null)
                message += $" Exception: {exception}";

            logService.AddLog(logLevel, category, message);
        }
    }
}