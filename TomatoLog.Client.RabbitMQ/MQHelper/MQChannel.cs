using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomatoLog.Client.RabbitMQ.MQHelper
{
    public class MQChannel
    {
        #region identity
        public string ExchangeTypeName { get; set; }
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutekeyName { get; set; }

        public IConnection Connection { get; set; }
        public EventingBasicConsumer Consumer { get; set; }
        /// <summary>
        ///  外部订阅消费者通知委托
        /// </summary>
        public Action<MessageBody> OnReceivedCallback { get; set; }
        #endregion

        /// <summary>
        ///  向当前队列发送消息
        /// </summary>
        /// <param name="content"></param>
        public void Publish(string content)
        {
            byte[] body = MQConnection.UTF8.GetBytes(content);
            IBasicProperties prop = new BasicProperties
            {
                DeliveryMode = 1
            };
            Consumer.Model.BasicPublish(this.ExchangeName, this.RoutekeyName, false, prop, body);
        }

        internal void Receive(object sender, BasicDeliverEventArgs e)
        {
            MessageBody msgBody = new MessageBody();
            try
            {
                string content = MQConnection.UTF8.GetString(e.Body);
                msgBody.Content = content;
                msgBody.Consumer = (EventingBasicConsumer)sender;
                msgBody.BasicDeliver = e;
            }
            catch (Exception ex)
            {
                msgBody.ErrorMessage = $"订阅-出错{ex.Message}";
                msgBody.Exception = ex;
                msgBody.Error = true;
                msgBody.Code = 500;
            }
            OnReceivedCallback?.Invoke(msgBody);
        }

        /// <summary>
        ///  设置消息处理完成标志
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="deliveryTag"></param>
        /// <param name="multiple"></param>
        public void SetBasicAck(EventingBasicConsumer consumer, ulong deliveryTag, bool multiple)
        {
            consumer.Model.BasicAck(deliveryTag, multiple);
           
        }

        /// <summary>
        ///  关闭消息队列的连接
        /// </summary>
        public void Stop()
        {
            if (this.Connection != null && this.Connection.IsOpen)
            {
                this.Connection.Close();
                this.Connection.Dispose();
            }
        }
    }
}
