using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoLog.Server.MQHelper
{
    /// <summary>
    ///  
    /// </summary>
    public class MQConnection
    {
        #region Identity

        public event EventHandler<BasicDeliverEventArgs> Received;

        public event EventHandler<ConsumerEventArgs> Registered;

        public event EventHandler<ShutdownEventArgs> Shutdown;

        public event EventHandler<ConsumerEventArgs> Unregistered;

        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;

        private string username = string.Empty;
        private string password = string.Empty;
        private string host = string.Empty;
        private int port = 5672;
        private string vhost = string.Empty;

        private IConnection connection = null;
        private ILogger _logger = null;
        public static UTF8Encoding UTF8 { get; set; } = new UTF8Encoding(false);
        #endregion

        public MQConnection(string userName, string password, string host, int port, string vhost, ILogger logger)
        {
            this.username = userName;
            this.password = password;
            this.host = host;
            this.port = port;
            this.vhost = vhost;
            this._logger = logger;

        }

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

        public MQChannel CreateReceiveChannel(string exchangeType, string exchange, string queue, string routekey)
        {
            IModel model = this.CreateModel(exchangeType, exchange, queue, routekey);
            model.BasicQos(0, 1, false);
            EventingBasicConsumer consumer = this.Receive(model, queue);
            consumer.Registered += (object sender, ConsumerEventArgs e) => { _logger?.LogDebug($"已注册消费队列，{e.ConsumerTags}"); };
            consumer.Shutdown += (object sender, ShutdownEventArgs e) => { _logger?.LogDebug($"已关闭消费队列，{e.ReplyCode}，{e.ReplyText}"); };
            consumer.ConsumerCancelled += (object sender, ConsumerEventArgs e) => { _logger?.LogDebug($"已退出消费队列，{e.ConsumerTags}"); };

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

        public IModel CreateModel(string type, string exchange, string queue, string routeKey, IDictionary<string, object> arguments = null)
        {
            type = string.IsNullOrEmpty(type) ? "default" : type;
            IModel model = this.Connection.CreateModel();
            model.BasicQos(0, 1, false);
            model.QueueDeclare(queue, true, false, false, arguments);
            model.QueueBind(queue, exchange, routeKey);
            return model;
        }

        public void Publish(IModel model, string exchange, string routeKey, string message, IBasicProperties properties = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            byte[] data = Encoding.UTF8.GetBytes(message);
            model.BasicPublish(exchange, routeKey, properties, data);
        }

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

        public void Close()
        {
            if (connection != null && connection.IsOpen)
            {
                connection.Close();
            }
        }
    }
}