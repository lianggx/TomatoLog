using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TomatoLog.Common.Config;
using TomatoLog.Common.Utilities;

namespace TomatoLog.Client.Kafka
{
    public class TomatoLogClientKafka : TomatoLogClient
    {
        private LogClient logClient;
        private EventKafkaOptions optionsRedis;
        public TomatoLogClientKafka(EventKafkaOptions options) : base(options)
        {
            Check.NotNull(options.BootstrapServers, nameof(options.BootstrapServers));
            Check.NotNull(options.Topic, nameof(options.Topic));

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
                await logClient.SendMessage(log);
            }
            catch (Exception ex)
            {
                if (optionsRedis.Logger != null)
                    optionsRedis.Logger.LogError(ex, ex.Message);
                else
                    throw;
            }
        }

        public override void WriteLog(int eventId, LogLevel logLevel, string message, string content = null, object extra = null)
        {
            var task = WriteLogAsync(eventId, logLevel, message, content, extra).ConfigureAwait(false).GetAwaiter();
            task.GetResult();
        }
    }
}
