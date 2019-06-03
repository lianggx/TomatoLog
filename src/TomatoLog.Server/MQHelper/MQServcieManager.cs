using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using TomatoLog.Common.Interface;
using TomatoLog.Common.Utilities;
using TomatoLog.Server.BLL;

namespace TomatoLog.Server.MQHelper
{
    public class MQServcieManager
    {
        private int timer_tick = 60 * 1000;
        private Timer timer = null;
        private ILogger logger = null;
        private IConfiguration cfg;
        private FilterService filterService;
        private ILogWriter logWriter;

        public MQServcieManager(ILogWriter logWriter, FilterService filterService, IConfiguration cfg, ILogger logger)
        {
            this.cfg = cfg;
            this.logger = logger;
            this.logWriter = logWriter;
            this.filterService = filterService;

            timer = new Timer(OnInterval, "", timer_tick, timer_tick);
        }

        private void OnInterval(object sender)
        {
            int count = 0;
            for (int i = 0; i < this.ChannelList.Count; i++)
            {
                var item = this.ChannelList[i];
                foreach (var c in item.List)
                {
                    if (c.Connection == null || c.Connection == null || !c.Connection.IsOpen)
                    {
                        count++;
                        try
                        {
                            c.Stop();
                            item.List.Remove(c);
                            var channel = item.CreateChannel(c.QueueName, c.RoutekeyName, c.ExchangeTypeName);
                            item.List.Add(channel);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($",{ex.Message},{ex.StackTrace}");
                            return;
                        }
                    }
                }
            }
            logger.LogInformation($"Check Complete!：{count}");
        }

        public void Start()
        {
            var key = "TomatoLog:Flow:RabbitMQ";
            var rabbitMQ = cfg.GetSection(key);
            Check.NotNull(rabbitMQ, key);
            MQSetting setting = cfg.GetSection(key).Get<MQSetting>();
            ChannelList = new List<IService>();
            for (int i = 0; i < setting.Channels; i++)
            {
                var ls = new LogService(logWriter, filterService, setting, logger);
                ls.Start();

                Console.WriteLine("已连接RabbitMQ...");
                ChannelList.Add(ls);
            }
        }

        public void Stop()
        {
            foreach (var item in this.ChannelList)
            {
                item.Stop();
            }
            ChannelList.Clear();
            timer.Dispose();
        }

        public List<IService> ChannelList { get; set; }
    }
}
