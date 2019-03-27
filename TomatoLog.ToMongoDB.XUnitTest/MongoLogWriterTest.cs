using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace TomatoLog.ToMongoDB.XUnitTest
{
    public class MongoLogWriterTest
    {
        ILogWriter writer;
        public MongoLogWriterTest()
        {
            StorageOptions options = new StorageOptions
            {
                MongoDB = "mongodb://root:root@172.16.1.220:27017/admin",
                Type = TomatoLog.Common.Utilities.StorageType.ToMongoDB
            };
            var logger = new LoggerFactory().CreateLogger<MongoLogWriterImpl>();
            writer = new MongoLogWriterImpl(options, logger);
        }


        [Fact]
        public async Task GetProjects()
        {
            var result = await writer.GetProjects();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task List()
        {
            var result = await writer.List("TomatoLog-Server", "Error");
            Assert.NotNull(result);
        }
    }
}
