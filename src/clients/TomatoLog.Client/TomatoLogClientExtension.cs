using TomatoLog.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TomatoLog.Common.Interface;
using Microsoft.Extensions.Options;
using TomatoLog.Common.Config;
using Microsoft.Extensions.Configuration;

namespace TomatoLog.Client.Extensions
{
    public static class TomatoLogClientExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="eventId">0=Not specified</param>
        /// <returns></returns>
        public static async Task AddTomatoLogAsync(this Exception exception, int eventId = 0)
        {
            if (TomatoLogClient.Instance == null)
                throw new ArgumentNullException(nameof(TomatoLogClient.Instance));

            var stackTrace = GetStackTrace(new StringBuilder(), exception);
            await TomatoLogClient.Instance.WriteLogAsync(eventId, LogLevel.Error, exception.Message, stackTrace, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="eventId">0=Not specified</param>
        /// <returns></returns>
        public static void AddTomatoLog(this Exception exception, int eventId = 0)
        {
            var task = exception.AddTomatoLogAsync(eventId).ConfigureAwait(false).GetAwaiter();
            task.GetResult();
        }

        private static string GetStackTrace(StringBuilder sb, Exception ex)
        {
            sb.AppendFormat("{0}|{1}", ex.Message, ex.StackTrace);
            var data = ex.Data;
            if (data != null && data.Count > 0)
            {
                sb.Append("[");
                foreach (var key in data.Keys)
                {
                    sb.AppendFormat("{0}:{1};", key, data[key]);
                }
                sb.Append("]");
            }

            if (ex.InnerException != null)
            {
                GetStackTrace(sb, ex.InnerException);
            }

            return sb.ToString();
        }

        public static ILoggingBuilder UseTomatoLogger(this ILoggingBuilder builder, Action<ITomatoLogClient> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            builder.Services.Configure(configure);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TomatoLogClientLoggerProvider>());

            return builder;
        }

        public static ILoggerFactory UseTomatoLogger(this ILoggerFactory factory, ITomatoLogClient logClient)
        {
            if (logClient == null)
                throw new ArgumentNullException(nameof(logClient));

            factory.AddProvider(new TomatoLogClientLoggerProvider(logClient));
            return factory;
        }
    }
}
