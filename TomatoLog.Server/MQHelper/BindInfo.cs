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
        /// <summary>
        ///  创建通道
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="routeKey">路由名称</param>
        /// <param name="exchangeType">交换机类型</param>
        /// <returns></returns>
        MQChannel CreateChannel(string queue, string routeKey, string exchangeType);
        /// <summary>
        ///  开启订阅
        /// </summary>
        void Start();
        /// <summary>
        ///  停止订阅
        /// </summary>
        void Stop();
        /// <summary>
        ///  发送错误消息
        /// </summary>
        /// <param name="content"></param>
        void SendWarning(string content);
        /// <summary>
        ///  通道列表
        /// </summary>
        List<MQChannel> List { get; set; }
        /// <summary>
        ///  访问消息队列的用户名
        /// </summary>
        string UserName { get; }
        /// <summary>
        ///  访问消息队列的密码
        /// </summary>
        string Password { get; }
        /// <summary>
        ///  消息队列的主机地址
        /// </summary>
        string Host { get; }
        /// <summary>
        ///  消息队列的主机开放的端口
        /// </summary>
        int Port { get; }
        /// <summary>
        ///  消息队列中定义的虚拟机
        /// </summary>
        string vHost { get; }
        /// <summary>
        ///  消息队列中定义的交换机
        /// </summary>
        string Exchange { get; }
        /// <summary>
        ///  定义的队列列表
        /// </summary>
        List<BindInfo> Binds { get; }
    }

    public partial class BindInfo
    {
        /// <summary>
        ///  队列名称
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        ///  路由名称
        /// </summary>
        public string RouterKey { get; set; }
        /// <summary>
        ///  交换机类型
        /// </summary>
        public string ExchangeType { get; set; }
        /// <summary>
        ///  订阅回调函数
        /// </summary>
        public Action<MessageBody> OnReceived { get; set; }
    }

    public class MessageBody
    {
        public EventingBasicConsumer Consumer { get; set; }
        public BasicDeliverEventArgs BasicDeliver { get; set; }
        /// <summary>
        ///  0成功
        /// </summary>
        public int Code { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public bool Error { get; set; }
        public Exception Exception { get; set; }
    }
}
