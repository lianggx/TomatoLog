using Microsoft.Extensions.Logging;
using System;
using TomatoLog.Common.Interface;

namespace TomatoLog.Client
{
    public class TomatoLogClientLogger : ILogger
    {
        public ITomatoLogClient LogClient { get; set; }
        public IDisposable BeginScope<TState>(TState state) => this.ScopeProvider?.Push(state) ?? default;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (LogClient == null)
            {
                throw new ArgumentNullException(nameof(LogClient));
            }

            if (IsEnabled(logLevel))
            {
                var message = formatter(state, exception);
                if (!string.IsNullOrEmpty(message) || exception != null)
                {
                    LogClient.WriteLog(eventId.Id, logLevel, message, exception?.ToString());
                }
            }
        }

        public IExternalScopeProvider ScopeProvider { get; set; }
    }
}
