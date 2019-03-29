using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomatoLog.Server.MQHelper
{
    public class MQSetting
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string vHost { get; set; }
        public string Exchange { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        public string RouteKey { get; set; }
        public int Channels { get; set; }
    }

    public interface IService
    {
        MQChannel CreateChannel(string queue, string routeKey, string exchangeType);
        void Start();
        void Stop();
        void SendWarning(string content);
        List<MQChannel> List { get; set; }
        string UserName { get; }
        string Password { get; }
        string Host { get; }
        int Port { get; }
        string vHost { get; }
        string Exchange { get; }
        List<BindInfo> Binds { get; }
    }

    public partial class BindInfo
    {
        public string Queue { get; set; }
        public string RouterKey { get; set; }
        public string ExchangeType { get; set; }
        public Action<MessageBody> OnReceived { get; set; }
    }

    public class MessageBody
    {
        public EventingBasicConsumer Consumer { get; set; }
        public BasicDeliverEventArgs BasicDeliver { get; set; }
        /// <summary>
        ///  0=Success
        /// </summary>
        public int Code { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public bool Error { get; set; }
        public Exception Exception { get; set; }
    }
}
