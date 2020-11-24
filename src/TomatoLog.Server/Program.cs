using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TomatoLog.Common.Utilities;
using System.Text.Json.Serialization;

namespace TomatoLog.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var hostport = new ConfigurationBuilder().AddJsonFile("hosting.json").Build();
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseConfiguration(hostport);
                    webBuilder.UseKestrel();
                });
    }

    public class TestModel
    {
        public string[] IPList { get; set; }
        public LogLevel Level { get; set; }
        public DateTime DT { get; set; }
    }
}
