using System;
using System.Threading.Tasks;
using TomatoLog.Client.RabbitMQ;
using TomatoLog.Client.Extensions;
using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using Xunit;

namespace TomatoLog.ClientRabbitMQ.XUnitTest
{
    public class TomatoLogClientRabbitMQTest
    {
        ITomatoLogClient client;
        public TomatoLogClientRabbitMQTest()
        {
            EventRabbitMQOptions options = new EventRabbitMQOptions
            {
                Logger = null,
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                ProjectLabel = "20272",
                ProjectName = "TomatoLog",
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
                Version = "1.0.1",
                Exchange = "TomatoLog-Exchange",
                ExchangeType = "direct",
                Host = "127.0.0.1",
                Password = "123456",
                Port = 5672,
                QueueName = "TomatoLog-Queue",
                RouteKey = "All",
                UserName = "admin",
                vHost = "TomatoLog"
            };
            client = new TomatoLogClientRabbitMQ(options);
        }

        [Fact]
        public async Task WriteLogAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                //await client.WriteLogAsync(LogLevel.Debug, "ES Exception", null, null);
                //await client.WriteLogAsync(LogLevel.Information, "ES Exception", null, null);
                //await client.WriteLogAsync(LogLevel.Critical, "ES Exception", null, null);
                //await client.WriteLogAsync(LogLevel.None, "ES Exception", null, null);
                //await client.WriteLogAsync(LogLevel.Trace, "ES Exception", null, null);
                //await client.WriteLogAsync(LogLevel.Warning, "ES Exception", null, null);
                // await client.WriteLogAsync(LogLevel.Error, "ES Exception", null, null);
                try
                {
                    throw new Exception("RabbitMQ throw exception");
                }
                catch (Exception ex)
                {
                    await ex.AddTomatoLogAsync(1320);
                }
            }
        }
    }
}
