using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;

namespace TomatoLog.Client
{
    public class TomatoLogClientLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, TomatoLogClientLogger> loggers;
        private IExternalScopeProvider scopeProvider;
        private readonly ITomatoLogClient logClient;

        public TomatoLogClientLoggerProvider(ITomatoLogClient logClient)
        {
            this.logClient = logClient;
            loggers = new ConcurrentDictionary<string, TomatoLogClientLogger>();
        }

        ~TomatoLogClientLoggerProvider()
        {
            if (!IsDisposed)
                Dispose(true);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, loggerName => new TomatoLogClientLogger
            {
                ScopeProvider = this.scopeProvider,
                LogClient = this.logClient
            });
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                this.IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void Dispose(bool disposing)
        {
            if (TomatoLogClient.Instance != null)
                TomatoLogClient.Instance = null;
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;

            foreach (var logger in loggers)
            {
                logger.Value.ScopeProvider = this.scopeProvider;
            }
        }

        public bool IsDisposed { get; set; }
    }
}
