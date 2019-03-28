using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using TomatoLog.Common.Utilities;
using TomatoLog.Server.BLL;
using TomatoLog.Server.MQHelper;

namespace TomatoLog.Server.Extensions
{
    public static class StartupExtension
    {
        private static ILoggerFactory logFactory = null;
        private static ILogWriter logWriter = null;
        private static SysConfigManager sysConfigManager = null;
        private static FilterService filterService = null;
        private static IApplicationLifetime lifeTime = null;
        private static ILogger logger = null;

        public static IServiceCollection AddLogWriter(this IServiceCollection service, IConfiguration configuration)
        {
            logFactory = new LoggerFactory().AddNLog();
            logger = logFactory.CreateLogger<ILogWriter>();
            StorageOptions storage = configuration.GetSection("TomatoLog:Storage").Get<StorageOptions>();
            Check.NotNull(storage, nameof(StorageOptions));

            try
            {
                var storageType = $"TomatoLog.{storage.Type}";
                //var plugin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", storageType, storageType + ".dll");
                //var assembly = Assembly.LoadFrom(plugin);
                var plugin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", storageType);
                var dlls = Directory.GetFiles(plugin, "*.dll");
                Assembly assembly = null;
                foreach (var d in dlls)
                {
                    FileInfo fi = new FileInfo(d);
                    //  AppDomain.CurrentDomain.AssemblyResolve += (sender, args)=>{};
                    var aby = AssemblyLoadContext.Default.LoadFromAssemblyPath(fi.FullName);
                    var fileName = fi.Name.Substring(0, fi.Name.LastIndexOf('.'));
                    if (fileName == storageType)
                        assembly = aby;
                }
                foreach (var item in assembly.ExportedTypes)
                {
                    if (item.GetInterface(nameof(ILogWriter)) != null)
                    {
                        logWriter = (ILogWriter)assembly.CreateInstance(item.FullName, true, BindingFlags.Default, null, new object[] { storage, logger }, null, null);
                        break;
                    }
                }

                if (logWriter == null)
                    throw new ArgumentOutOfRangeException("必须指定  Storage Type");
                service.AddSingleton<ILogWriter>(logWriter);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
            return service;
        }

        public static IApplicationBuilder UseTomatoLog(this IApplicationBuilder app,
                                                       IConfiguration configuration,
                                                       IDistributedCache cache,
                                                       SysConfigManager sysManager,
                                                       ProConfigManager proManager,
                                                       IApplicationLifetime lifetime)
        {
            lifeTime = lifetime;
            sysConfigManager = sysManager;
            filterService = new FilterService(configuration, cache, sysManager, proManager, logger);
            var flowType = configuration.GetSection("TomatoLog:Flow:Type").Get<FlowType>();
            switch (flowType)
            {
                default:
                    UseRedis(configuration, cache);
                    break;
                case FlowType.RabbitMQ:
                    UseRabbitMQ(configuration);
                    break;
                case FlowType.Kafaka:
                    throw new NotImplementedException();
            }
            return app;
        }

        private static void UseRedis(IConfiguration configuration, IDistributedCache cache)
        {
            var connectionString = configuration["TomatoLog:Flow:Redis:Connection"];
            var channel = configuration["TomatoLog:Flow:Redis:Channel"];
            Check.NotNull(connectionString, "TomatoLog:Flow:Redis:Connection");
            Check.NotNull(channel, "TomatoLog:Flow:Redis:Channel");

            RedisHelper.Initialization(new CSRedis.CSRedisClient(connectionString));
            RedisHelper.Subscribe((channel, data =>
                {
                    try
                    {
                        LogMessage log = JsonConvert.DeserializeObject<LogMessage>(data.Body);
                        logWriter.Write(log);
                        filterService.Filter(log);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message + "|" + data.Body, ex);
                    }
                }
            ));
        }

        private static MQServcieManager mqManager = null;
        private static void UseRabbitMQ(IConfiguration configuration)
        {
            mqManager = new MQServcieManager(logWriter, filterService, configuration, logger);
            lifeTime.ApplicationStarted.Register(() =>
            {
                mqManager.Start();
            });
            lifeTime.ApplicationStopping.Register(() =>
            {
                mqManager.Stop();
            });
        }


    }
}