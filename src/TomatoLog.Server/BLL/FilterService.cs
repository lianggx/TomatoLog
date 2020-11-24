using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using TomatoLog.Common.Utilities;
using TomatoLog.Server.Models;

namespace TomatoLog.Server.BLL
{
    public class FilterService
    {
        private readonly IConfiguration configuration;
        private readonly IDistributedCache cache;
        private readonly SysConfigManager sysManager;
        private readonly ProConfigManager proManager;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public FilterService(IConfiguration configuration,
                                        IDistributedCache cache,
                                        SysConfigManager sysManager,
                                        ProConfigManager proManager,
                                        ILogger logger,
                                        HttpClient httpClient)
        {
            this.configuration = configuration;
            this.cache = cache;
            this.sysManager = sysManager;
            this.proManager = proManager;
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async void Filter(LogMessage log)
        {
            if (log == null)
            {
                logger.LogError("The Message was null.");
                return;
            }
            ReportViewModel reportModel = proManager.ConfigObject?.FirstOrDefault(f => f.Setting.ProjectName == log.ProjectName);
            var sett = reportModel?.Setting;
            if (reportModel == null || !sett.On)
            {
                reportModel = sysManager.ConfigObject;
                sett = reportModel.Setting;
            }

            if (sett.Levels != null && sett.Levels.Contains(log.LogLevel.ToString()))
            {
                var key = $"{log.ProjectName}_{log.LogLevel}";
                int.TryParse(await cache.GetStringAsync(key), out int count);
                if ((count + 1) >= sett.Count)
                {
                    Notifier(reportModel, configuration, log);
                    await cache.RemoveAsync(key);
                }
                else
                {
                    if (count == 0)
                    {
                        var options = new DistributedCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(sett.Time)
                        };
                        await cache.SetStringAsync(key, (++count).ToString(), options);
                    }
                    else
                        await cache.SetStringAsync(key, (++count).ToString());
                }
            }
        }

        private void Notifier(ReportViewModel repportModel, IConfiguration configuration, LogMessage log)
        {
            var notify = new NotifyMonitor(httpClient, configuration, logger, repportModel);
            notify.SendSms(log);
            notify.SendEmail(log);
        }
    }
}
