using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            throw new NotImplementedException("WriteLogAsync");
        }

        public virtual void WriteLog(int eventId, LogLevel logLevel, string message, string content = null, object extra = null)
        {
            throw new NotImplementedException("WriteLog");
        }

        public async Task<string> CreateLog(int eventId, LogLevel logLevel, string message, string content, object extra = null)
        {
            var opt = TomatoLogClient.Instance.Options;
            var sysOpt = opt.SysOptions;
            var hostName = Dns.GetHostName();
            var process = Process.GetCurrentProcess();

            Hashtable log = new Hashtable()
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
                var ips = GetHostAddress();
                log["IP"] = ips.FirstOrDefault();
                if (sysOpt.IPList)
                    log["IPList"] = ips;
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
                log["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ms");
            if (sysOpt.UserName)
                log["UserName"] = Environment.UserName;
            if (sysOpt.ErrorMessage)
                log["ErrorMessage"] = message;
            if (sysOpt.StackTrace)
                log["StackTrace"] = content;

            if (extra != null)
                log["Extra"] = extra;

            var json = JsonSerializer.Serialize(log);

            return json;
        }

        private List<string> GetHostAddress()
        {
            List<string> result = new List<string>();
            var ipList = NetworkInterface.GetAllNetworkInterfaces()
                                         .Where(f => f.NetworkInterfaceType != NetworkInterfaceType.Loopback && f.OperationalStatus == OperationalStatus.Up).ToList();

            foreach (var item in ipList)
            {
                var props = item.GetIPProperties();
                var ipv4 = props.UnicastAddresses
                                .Where(f => f.Address.AddressFamily == AddressFamily.InterNetwork)
                                .Select(f => f.Address)
                                .FirstOrDefault();

                if (ipv4 == null) continue;
                result.Add(ipv4.ToString());
            }
            return result;
        }
    }
}
