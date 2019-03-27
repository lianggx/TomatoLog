using System;
using RabbitMQ.Client;
using RabbitMQ.Util;
using System.Text;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TomatoLog.Server.MQHelper
{
    /// <summary>
    ///  
    /// </summary>
    public class MQConnection
    {
        #region Identity
        /// <summary>
        ///  消费者接收到消息事件
        /// </summary>
        public event EventHandler<BasicDeliverEventArgs> Received;
        /// <summary>
        ///  消费者注册事件
        /// </summary>
        public event EventHandler<ConsumerEventArgs> Registered;
        /// <summary>
        ///  消费者关闭事件
        /// </summary>
        public event EventHandler<ShutdownEventArgs> Shutdown;
        /// <summary>
        ///  消费者取消注册事件
        /// </summary>
        public event EventHandler<ConsumerEventArgs> Unregistered;
        /// <summary>
        ///  消费者退出事件
        /// </summary>
        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;

        private string username = string.Empty;
        private string password = string.Empty;
        private string host = string.Empty;
        private int port = 5672;
        private string vhost = string.Empty;

        private IConnection connection = null;
        private ILogger _logger = null;
        /// <summary>
        ///  构造无 utf8 标记的编码转换器
        /// </summary>
        public static UTF8Encoding UTF8 { get; set; } = new UTF8Encoding(false);
        #endregion

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="host">远程 rabbitmq 主机地址</param>
        /// <param name="port">端口</param>
        /// <param name="vhost">虚拟机名称</param>
        public MQConnection(string userName, string password, string host, int port, string vhost, ILogger logger)
        {
            this.username = userName;
            this.password = password;
            this.host = host;
            this.port = port;
            this.vhost = vhost;
            this._logger = logger;

        }

        /// <summary>
        /// 消息队列连接，该连接必须手动管理
        /// </summary>
        /// <returns></returns>
        public IConnection Connection
        {
            get
            {
                if (connection == null || !connection.IsOpen)
                {
                    ConnectionFactory cf = new ConnectionFactory
                    {
                        AutomaticRecoveryEnabled = true,
                        UserName = this.username,
                        Password = this.password,
                        HostName = this.host,
                        VirtualHost = this.vhost,
                        Port = this.port
                    };
                    connection = cf.CreateConnection();
                }

                return connection;
            }
        }

        /// <summary>
        ///  创建消息通道
        /// </summary>
        /// <param name="cfg"></param>
        public MQChannel CreateReceiveChannel(string exchangeType, string exchange, string queue, string routekey)
        {
            IModel model = this.CreateModel(exchangeType, exchange, queue, routekey);
            model.BasicQos(0, 1, false);
            EventingBasicConsumer consumer = this.Receive(model, queue);
            consumer.Registered += (object sender, ConsumerEventArgs e) => { _logger?.LogDebug($"已注册消费队列，{e.ConsumerTag}"); };
            consumer.Shutdown += (object sender, ShutdownEventArgs e) => { _logger?.LogDebug($"已关闭消费队列，{e.ReplyCode}，{e.ReplyText}"); };
            consumer.ConsumerCancelled += (object sender, ConsumerEventArgs e) => { _logger?.LogDebug($"已退出消费队列，{e.ConsumerTag}"); };

            MQChannel channel = new MQChannel()
            {
                Connection = this.connection,
                Consumer = consumer,
                ExchangeName = exchange,
                ExchangeTypeName = exchangeType,
                QueueName = queue,
                RoutekeyName = routekey
            };
            consumer.Received += channel.Receive;

            return channel;
        }

        /// <summary>
        ///  创建一个通道，包含交换机/队列/路由，并建立绑定关系
        /// </summary>
        /// <param name="type">交换机类型</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routeKey">路由名称</param>
        /// <returns></returns>
        public IModel CreateModel(string type, string exchange, string queue, string routeKey, IDictionary<string, object> arguments = null)
        {
            type = string.IsNullOrEmpty(type) ? "default" : type;
            IModel model = this.Connection.CreateModel();
            model.BasicQos(0, 1, false);
            model.QueueDeclare(queue, true, false, false, arguments);
            model.QueueBind(queue, exchange, routeKey);
            return model;
        }

        /// <summary>
        ///  发布消息到队列中
        /// </summary>
        /// <param name="model">消息通道</param>
        /// <param name="exchange">交换机名称</param>
        /// <param name="routeKey">路由名称</param>
        /// <param name="message">待发送到消息</param>
        /// <param name="properties">附加属性，比如TTL</param>
        /// <returns></returns>
        public void Publish(IModel model, string exchange, string routeKey, string message, IBasicProperties properties = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            byte[] data = Encoding.UTF8.GetBytes(message);
            model.BasicPublish(exchange, routeKey, properties, data);
        }

        /// <summary>
        ///  接收消息到队列中
        /// </summary>
        /// <param name="model">消息通道</param>
        /// <param name="queue">队列名称</param>
        /// <param name="callback">订阅消息的回调事件</param>
        /// <returns></returns>
        public EventingBasicConsumer Receive(IModel model, string queue)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(model);
            model.BasicConsume(queue, false, consumer);
            consumer.Received += Received;
            consumer.Registered += Registered;
            consumer.Unregistered += Unregistered;
            consumer.Shutdown += Shutdown;
            consumer.ConsumerCancelled += ConsumerCancelled;

            return consumer;
        }

        /// <summary>
        ///  关闭当前连接
        /// </summary>
        public void Close()
        {
            if (connection != null && connection.IsOpen)
            {
                connection.Close();
            }
        }
    }
}