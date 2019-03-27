using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using TomatoLog.Common.Utilities;

namespace TomatoLog.Server.BLL
{
    public class FilterService
    {
        private readonly IConfiguration configuration;
        private readonly IDistributedCache cache;
        private readonly SysConfigManager sysManager;
        private readonly ILogger logger;

        public FilterService(IConfiguration configuration, IDistributedCache cache, SysConfigManager sysManager, ILogger logger)
        {
            this.configuration = configuration;
            this.cache = cache;
            this.sysManager = sysManager;
            this.logger = logger;
        }

        public async void Filter(LogMessage log)
        {
            var reportSetting = sysManager.Report.Setting;
            if (reportSetting.On)
            {
                if (reportSetting.Levels.Contains(log.LogLevel.ToString()))
                {
                    var key = $"{log.ProjectName}_{log.LogLevel}";
                    var count = await cache.GetObjectAsync<int>(key);
                    if (count >= reportSetting.Count)
                        Notifier(configuration, log);
                    else
                    {
                        if (count == 0)
                        {
                            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(reportSetting.Time)
                            };
                            await cache.SetObjectAsync(key, ++count, options);
                        }
                        else
                            await cache.SetObjectAsync(key, ++count);
                    }
                }
            }
        }

        private void Notifier(IConfiguration configuration, LogMessage log)
        {
            var client = HttpClientFactory.Create();

            var notify = new NotifyMonitor(client, configuration, logger, sysManager.Report);
            notify.SendSms(log);
            notify.SendEmail(log);
        }
    }
}
