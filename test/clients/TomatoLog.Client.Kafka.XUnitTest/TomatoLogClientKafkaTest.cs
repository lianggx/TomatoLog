using System;
using System.Threading.Tasks;
using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using Xunit;
using TomatoLog.Client.Extensions;

namespace TomatoLog.Client.Kafka.XUnitTest
{
    public class TomatoLogClientKafkaTest
    {
        ITomatoLogClient client;
        public TomatoLogClientKafkaTest()
        {
            var options = new EventKafkaOptions
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
                BootstrapServers = "127.0.0.1:9092",
                Topic = "TomatoLog"
            };
            client = new TomatoLogClientKafka(options);
        }

        [Fact]
        public async Task WriteLogAsync()
        {
            int i = 0;
            for (; i < 10; i++)
            {
                try
                {
                    throw new Exception("Kafka throw exception");
                }
                catch (Exception ex)
                {
                    ex.Data["connection"] = "127.0.0.1";
                    await ex.AddTomatoLogAsync(1320);
                }
            }

            Assert.Equal(10, i);
        }
    }
}
