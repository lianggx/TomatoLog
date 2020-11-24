using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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
        private static IHostApplicationLifetime lifeTime = null;
        private static ILogger logger = null;

        public static IServiceCollection AddCustomerDistributedCache(this IServiceCollection services, IConfiguration configuration)
        {
            var distributedConfig = configuration["TomatoLog:Cache-Redis"];
            if (string.IsNullOrEmpty(distributedConfig))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = distributedConfig;
                    options.InstanceName = "TomatoLog";
                    options.ConfigurationOptions.ClientName = "TomatoLogServer";
                });
            }

            return services;
        }

        public static IServiceCollection AddLogWriter(this IServiceCollection service, IConfiguration configuration)
        {
            logFactory = new LoggerFactory().AddNLog();
            logger = logFactory.CreateLogger<ILogWriter>();
            StorageOptions storage = configuration.GetSection("TomatoLog:Storage").Get<StorageOptions>();
            Check.NotNull(storage, nameof(StorageOptions));

            try
            {
                var storageType = $"TomatoLog.{storage.Type}";
                var plugin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", storageType);
                var dlls = Directory.GetFiles(plugin, "*.dll");
                Assembly assembly = null;
                foreach (var d in dlls)
                {
                    FileInfo fi = new FileInfo(d);
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
                    throw new ArgumentOutOfRangeException(paramName: "The Storage:Type required!");

                service.AddSingleton(logWriter);
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
                                                       IHostApplicationLifetime lifetime,
                                                       HttpClient httpClient)
        {
            lifeTime = lifetime;
            sysConfigManager = sysManager;
            filterService = new FilterService(configuration, cache, sysManager, proManager, logger, httpClient);
            var flowType = configuration.GetSection("TomatoLog:Flow:Type").Get<FlowType>();
            switch (flowType)
            {
                default:
                    UseRedis(configuration);
                    break;
                case FlowType.RabbitMQ:
                    UseRabbitMQ(configuration);
                    break;
                case FlowType.Kafka:
                    UseKafka(configuration);
                    break;
            }
            return app;
        }

        private static ConnectionMultiplexer multiplexer;
        private static void UseRedis(IConfiguration configuration)
        {
            var connectionString = configuration["TomatoLog:Flow:Redis:Connection"];
            var channelName = configuration["TomatoLog:Flow:Redis:Channel"];
            Check.NotNull(connectionString, "TomatoLog:Flow:Redis:Connection");
            Check.NotNull(channelName, "TomatoLog:Flow:Redis:Channel");

            multiplexer = ConnectionMultiplexer.Connect(connectionString);
            var channel = multiplexer.GetSubscriber().Subscribe(channelName);
            channel.OnMessage(message =>
            {
                try
                {
                    LogMessage log = JsonSerializer.Deserialize<LogMessage>(message.Message);
                    logWriter.Write(log);
                    filterService.Filter(log);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message + "|" + message.Message, ex);
                }
            });
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

        private readonly static CancellationTokenSource cts_kafka = new CancellationTokenSource();
        private static void UseKafka(IConfiguration configuration)
        {
            lifeTime.ApplicationStarted.Register(() =>
            {
                try
                {
                    var conf = new Confluent.Kafka.ConsumerConfig
                    {
                        GroupId = configuration["TomatoLog:Flow:Kafka:Group"],
                        BootstrapServers = configuration["TomatoLog:Flow:Kafka:BootstrapServers"],
                        AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
                    };

                    using var consumer = new Confluent.Kafka.ConsumerBuilder<Confluent.Kafka.Ignore, string>(conf).Build();
                    var topic = configuration["TomatoLog:Flow:Kafka:Topic"];
                    consumer.Subscribe(topic);

                    logger.LogInformation($"Kafka Consume started.");

                    var cancellationToken = cts_kafka.Token;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        Confluent.Kafka.ConsumeResult<Confluent.Kafka.Ignore, string> result = null;
                        try
                        {
                            result = consumer.Consume(cancellationToken);
                            LogMessage log = JsonSerializer.Deserialize<LogMessage>(result.Message.Value);
                            logWriter.Write(log);
                            filterService.Filter(log);
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"Error occured: {e.Message}{e.StackTrace}");
                        }
                        finally
                        {
                            consumer.Commit(result);
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogError("{0}/{1}", ex.Message, ex.StackTrace);
                }
            });

            lifeTime.ApplicationStopping.Register(() =>
            {
                cts_kafka.Cancel();
                logger.LogInformation($"Kafka Consume stoped.");
            });

        }
    }
}