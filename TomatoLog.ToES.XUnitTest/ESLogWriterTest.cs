using TomatoLog.Common.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TomatoLog.ToES.XUnitTest
{
    public class ESLogWriterTest
    {
        ESLogWriterImpl writer;

        public ESLogWriterTest()
        {
            StorageOptions options = new StorageOptions
            {
                ES = "http://172.16.1.249:9200",
                Type = TomatoLog.Common.Utilities.StorageType.ToMongoDB
            };
            var logger = new LoggerFactory().CreateLogger<ESLogWriterImpl>();
            writer = new ESLogWriterImpl(options, logger);
        }

        [Fact]
        public async Task List()
        {
            string proj = "tomatolog-server";
            string logLevel = "error";
            JToken query = new JObject();
            query["multi_match"] = new JObject()
            {
                { "query","Ö÷¶¯Òì³£" }
            };
            var jar = new JArray();
            jar.Add("errormessage");
            jar.Add("stacktrace");
            query["multi_match"]["fields"] = jar;
            var keyword = "";// query.ToString();

            var result = await writer.List(proj, logLevel, keyword);

            Assert.NotNull(result);
        }
    }
}
