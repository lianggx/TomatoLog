using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using System;

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

        public Action<MessageBody> OnReceivedCallback { get; set; }
        #endregion

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
                msgBody.ErrorMessage = $"Subscribe-Exception | {ex.Message}";
                msgBody.Exception = ex;
                msgBody.Error = true;
                msgBody.Code = 500;
            }
            OnReceivedCallback?.Invoke(msgBody);
        }

        public void SetBasicAck(EventingBasicConsumer consumer, ulong deliveryTag, bool multiple)
        {
            consumer.Model.BasicAck(deliveryTag, multiple);

        }

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
