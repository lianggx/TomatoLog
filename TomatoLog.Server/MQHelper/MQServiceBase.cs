using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomatoLog.Server.MQHelper
{
    public abstract class MQServiceBase : IService
    {
        internal bool started = false;
        internal ILogger logger;
        internal MQChannel Channel { get; set; }
        internal MQSetting setting { get; set; }
        internal MQServiceBase(MQSetting setting, ILogger logger)
        {
            this.setting = setting;
            this.logger = logger;
            UserName = setting.UserName;
            Password = setting.Password;
            Host = setting.Host;
            Port = setting.Port;
            vHost = setting.vHost;
        }

        public MQChannel CreateChannel(string queue, string routeKey, string exchangeType)
        {
            MQConnection conn = new MQConnection(this.UserName, this.Password, this.Host, this.Port, this.vHost, this.logger);
            MQChannel channel = conn.CreateReceiveChannel(exchangeType, this.Exchange, queue, routeKey);
            return channel;
        }

        public MQConnection CreateConnection()
        {
            MQConnection conn = new MQConnection(this.UserName, this.Password, this.Host, this.Port, this.vHost, this.logger);
            return conn;
        }

        public void SendWarning(string content)
        {
            logger.LogError(content);
        }


        /// <summary>
        ///  启动订阅
        /// </summary>
        public void Start()
        {
            if (started)
            {
                Console.WriteLine("服务已经启动！");
                return;
            }
            foreach (var item in this.Binds)
            {
                Channel = CreateChannel(item.Queue, item.RouterKey, item.ExchangeType);
                Channel.OnReceivedCallback = item.OnReceived;
                this.List.Add(Channel);
            }
            started = true;
        }

        /// <summary>
        ///  停止订阅
        /// </summary>
        public void Stop()
        {
            foreach (var c in this.List)
            {
                logger.LogDebug("正在关闭消息通道,{0},{1},{2}", c.ExchangeName, c.QueueName, c.RoutekeyName);
                c.Stop();
                logger.LogDebug("已关闭消息通道,{0},{1},{2}", c.ExchangeName, c.QueueName, c.RoutekeyName);
            }
            this.List.Clear();
            started = false;
        }

        public List<MQChannel> List { get; set; } = new List<MQChannel>();

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string vHost { get; set; }
        public string Exchange { get; set; }
        public List<BindInfo> Binds { get; } = new List<BindInfo>();
    }
}
