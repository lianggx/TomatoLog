using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TomatoLog.Client.RabbitMQ.MQHelper;
using TomatoLog.Common.Config;
using TomatoLog.Common.Utilities;

namespace TomatoLog.Client.RabbitMQ
{
    public class TomatoLogClientRabbitMQ : TomatoLogClient
    {
        private LogClient logClient;
        private EventRabbitMQOptions optionsRedis;
        public TomatoLogClientRabbitMQ(EventRabbitMQOptions options) : base(options)
        {
            Check.NotNull(options.Host, nameof(options.Host));
            Check.NotNull(options.vHost, nameof(options.vHost));
            Check.NotNull(options.UserName, nameof(options.UserName));
            Check.NotNull(options.Password, nameof(options.Password));
            Check.NotNull(options.QueueName, nameof(options.QueueName));
            Check.NotNull(options.Exchange, nameof(options.Exchange));
            Check.NotNull(options.ExchangeType, nameof(options.ExchangeType));
            Check.NotNull(options.RouteKey, nameof(options.RouteKey));

            logClient = new LogClient(options);
            optionsRedis = options;
            Instance = this;
        }

        public override async Task WriteLogAsync(int eventId, LogLevel logLevel, string message, string content = null, object extra = null)
        {
            if (logLevel < this.Options.LogLevel) return;

            try
            {
                var log = await CreateLog(eventId, logLevel, message, content, extra);
                logClient.SendMessage(log);
            }
            catch (Exception ex)
            {
                if (optionsRedis.Logger != null)
                    optionsRedis.Logger.LogError(ex, ex.Message);
                else
                    throw ex;
            }
        }

        public override void WriteLog(int eventId, LogLevel logLevel, string message, string content = null, object extra = null)
        {
            var task = WriteLogAsync(eventId, logLevel, message, content, extra).ConfigureAwait(false).GetAwaiter();
            task.GetResult();
        }
    }
}
