using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TomatoLog.Common.Config;

namespace TomatoLog.Client.RabbitMQ.MQHelper
{
    public class LogClient
    {
        private EventRabbitMQOptions options;
        private static MQConnection mqc = null;
        private ILogger logger = null;

        public LogClient(EventRabbitMQOptions options)
        {
            this.options = options;
        }

        public void SendMessage(string message)
        {
            this.Connection.Publish(this.Channel, options.Exchange, options.RouteKey, message, null);
        }

        private MQConnection Connection
        {
            get
            {
                if (mqc == null || !mqc.Connection.IsOpen)
                {
                    mqc = new MQConnection(options.UserName, options.Password, options.Host, options.Port, options.vHost, logger);
                }
                return mqc;
            }
        }

        private static IModel channel = null;
        private IModel Channel
        {
            get
            {
                if (channel == null || channel.IsClosed)
                    channel = mqc.Connection.CreateModel();

                return channel;
            }
        }
    }
}
