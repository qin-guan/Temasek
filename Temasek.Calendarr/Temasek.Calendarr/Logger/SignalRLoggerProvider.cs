using Microsoft.AspNetCore.SignalR;
using Temasek.Calendarr.Hubs;

namespace Temasek.Calendarr.Logger;

public class SignalRLoggerProvider(IServiceProvider sp) : ILoggerProvider
{

  public ILogger CreateLogger(string categoryName)
  {
    return new SignalRLogger(categoryName, sp);
  }

  public void Dispose()
  {
    return;
  }

  private class SignalRLogger(string category, IServiceProvider sp) : ILogger
  {
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
      using var scope = sp.CreateScope();

      var hub = scope.ServiceProvider.GetRequiredService<IHubContext<LoggerHub>>();
      var message = formatter(state, exception);

      if (exception is not null)
        message += $" Exception: {exception}";

      _ = hub.Clients.All.SendAsync("ReceiveLog", logLevel, category, message);
    }
  }
}
