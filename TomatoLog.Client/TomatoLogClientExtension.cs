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

namespace TomatoLog.Client.Extensions
{
    public static class TomatoLogClientExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="eventId">0表示没有ID</param>
        /// <returns></returns>
        public static async Task AddTomatoLogAsync(this Exception exception, int eventId = 0)
        {
            var stackTrace = GetStackTrace(new StringBuilder(), exception);

            await TomatoLogClient.Instance.WriteLogAsync(eventId, LogLevel.Error, exception.Message, stackTrace, null);
        }

        private static string GetStackTrace(StringBuilder sb, Exception ex)
        {
            sb.AppendFormat("{0}#####{1}", ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
                GetStackTrace(sb, ex.InnerException);
            }

            return sb.ToString();
        }
    }
}
