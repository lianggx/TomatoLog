using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace TomatoLog.ToFile.XUnitTest
{
    public class FileLogWriterTest
    {
        ILogWriter writer;
        public FileLogWriterTest()
        {
            StorageOptions options = new StorageOptions
            {
                File = "D:\\TomatoLog\\Storage",
                Type = TomatoLog.Common.Utilities.StorageType.ToMongoDB
            };
            var logger = new LoggerFactory().CreateLogger<FileLogWriterImpl>();
            writer = new FileLogWriterImpl(options, logger);
        }


        [Fact]
        public async Task GetProjects()
        {
            var result = await writer.GetProjects();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetLabels()
        {
            var result = await writer.GetLabels("TomatoLog-Server");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task List()
        {
            var result = await writer.List("TomatoLog-Server", "Error-2019-03-19.log");
            Assert.NotNull(result);
        }
    }
}
