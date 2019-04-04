using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using TomatoLog.Common.Utilities;

namespace TomatoLog.Client
{
    public abstract class TomatoLogClient : ITomatoLogClient
    {
        public static ITomatoLogClient Instance { get; set; }
        public EventOptions Options { get; set; }

        protected TomatoLogClient(EventOptions options)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(options.ProjectName, nameof(options.ProjectName));

            this.Options = options;
            Instance = this;
        }

        public virtual Task WriteLogAsync(int eventId, LogLevel logLevel, string message, string content = null, object extra = null)
        {
            throw new NotImplementedException("WriteLog");
        }

        public async Task<string> CreateLog(int eventId, LogLevel logLevel, string message, string content, object extra = null)
        {
            var opt = TomatoLogClient.Instance.Options;
            var sysOpt = opt.SysOptions;
            var hostName = Dns.GetHostName();
            var process = Process.GetCurrentProcess();

            var log = new JObject()
            {
                {"ProjectName", opt.ProjectName },
                {"ProjectLabel", opt.ProjectLabel },
                {"LogLevel", logLevel.ToString()},
                {"Version", opt.Version}
            };

            if (sysOpt.EventId && eventId != 0)
                log["EventId"] = eventId;

            if (sysOpt.IP)
            {
                var ips = await Dns.GetHostAddressesAsync(hostName);
                var iplist = ips.Where(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).OrderByDescending(f => f.AddressFamily).ToList();
                log["IP"] = iplist[0]?.ToString();
                if (sysOpt.IPList)
                    log["IPList"] = JToken.FromObject(iplist.Select(a => a.ToString()));
            }
            if (sysOpt.MachineName)
                log["MachineName"] = hostName;
            if (sysOpt.ThreadId)
                log["ThreadId"] = Thread.CurrentThread.ManagedThreadId.ToString();
            if (sysOpt.ProcessId)
                log["ProcessId"] = process.Id.ToString();
            if (sysOpt.ProcessName)
                log["ProcessName"] = process.ProcessName;
            if (sysOpt.Timestamp)
                log["Timestamp"] = DateTime.Now.ToString();
            if (sysOpt.UserName)
                log["UserName"] = Environment.UserName;
            if (sysOpt.ErrorMessage)
                log["ErrorMessage"] = message;
            if (sysOpt.StackTrace)
                log["StackTrace"] = content;

            if (extra != null)
                log["Extra"] = JToken.FromObject(extra);

            return log.ToString();
        }
    }
}
