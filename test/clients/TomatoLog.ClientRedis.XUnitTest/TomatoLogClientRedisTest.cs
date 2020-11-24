using TomatoLog.Common;
using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TomatoLog.Client.Redis;
using TomatoLog.Client.Extensions;
using Xunit;

namespace TomatoLog.Client.XUnitTest
{
    public class TomatoLogClientRedisTest
    {
        ITomatoLogClient client;
        public TomatoLogClientRedisTest()
        {
            EventRedisOptions options = new EventRedisOptions
            {
                Channel = "TomatoLogChannel",
                ConnectionString = "127.0.0.1:6379,defaultDatabase=10,password=123456",
                Logger = null,
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                ProjectLabel = "20272",
                ProjectName = "TomatoLog-Server",
                SysOptions = new EventSysOptions
                {
                    EventId = true,
                    IP = true,
                    IPList = true,
                    MachineName = true,
                    ProcessId = true,
                    ProcessName = true,
                    ThreadId = true,
                    Timestamp = true,
                    UserName = true
                },
                Tags = null,
                Version = "1.0.1"
            };
            client = new TomatoLogClientRedis(options);
        }

        [Fact]
        public async Task WriteLogAsync()
        {
            for (int i = 0; i < 1000; i++)
            {
                await client.WriteLogAsync(1001, LogLevel.Debug, "ES Exception", null, null);
                await client.WriteLogAsync(1001, LogLevel.Information, "ES Exception", null, null);
                await client.WriteLogAsync(1001, LogLevel.Critical, "ES Exception", null, null);
                await client.WriteLogAsync(1001, LogLevel.None, "ES Exception", null, null);
                await client.WriteLogAsync(1001, LogLevel.Trace, "ES Exception", null, null);
                await client.WriteLogAsync(1001, LogLevel.Warning, "ES Exception", null, null);
                await client.WriteLogAsync(1001, LogLevel.Error, "ES Exception", null, null);
                //try
                //{
                //    throw new Exception("RabbitMQ throw exception");
                //}
                //catch (Exception ex)
                //{
                //    await ex.AddTomatoLogAsync(1320);
                //}
            }
        }
    }
}
