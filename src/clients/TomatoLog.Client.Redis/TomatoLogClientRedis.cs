using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TomatoLog.Common.Config;
using TomatoLog.Common.Utilities;

namespace TomatoLog.Client.Redis
{
    public class TomatoLogClientRedis : TomatoLogClient
    {
        private EventRedisOptions optionsRedis;
        public TomatoLogClientRedis(EventRedisOptions options) : base(options)
        {
            Check.NotNull(options.ConnectionString, nameof(options.ConnectionString));
            Check.NotNull(options.Channel, nameof(options.Channel));

            optionsRedis = options;
            RedisHelper.Initialization(new CSRedis.CSRedisClient(options.ConnectionString));
            Instance = this;
        }

        public override async Task WriteLogAsync(int eventId, LogLevel logLevel, string message, string content = null, object extra = null)
        {
            if (logLevel < this.Options.LogLevel) return;

            try
            {
                var log = await CreateLog(eventId, logLevel, message, content, extra);
                await RedisHelper.PublishAsync(optionsRedis.Channel, log);
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
