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
                ConnectionString = "172.16.1.250:6379,defaultDatabase=13,password=Gworld2017,prefix=TomatoLog,abortConnect=false",
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
            for (int i = 0; i < 10; i++)
            {
                //await client.WriteLogAsync(LogLevel.Debug, "ES Exception 第一次测试消息", null, null);
                //await client.WriteLogAsync(LogLevel.Information, "ES Exception 第一次测试消息", null, null);
                //await client.WriteLogAsync(LogLevel.Critical, "ES Exception 第一次测试消息", null, null);
                //await client.WriteLogAsync(LogLevel.None, "ES Exception 第一次测试消息", null, null);
                //await client.WriteLogAsync(LogLevel.Trace, "ES Exception 第一次测试消息", null, null);
                //await client.WriteLogAsync(LogLevel.Warning, "ES Exception 第一次测试消息", null, null);
                // await client.WriteLogAsync(LogLevel.Error, "ES Exception 第一次测试消息", null, null);
                try
                {
                    throw new Exception("Redis主动异常");
                }
                catch (Exception ex)
                {
                    await ex.AddTomatoLogAsync(1320);
                }
            }
        }
    }
}
