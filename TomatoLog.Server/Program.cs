using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.Threading.Tasks;

namespace TomatoLog.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                BuildWebHost(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }

        public static IWebHostBuilder BuildWebHost(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("hosting.json").Build();

            return WebHost.CreateDefaultBuilder(args)
                  .UseStartup<Startup>()
                  .UseConfiguration(builder)
                  .ConfigureLogging(logging =>
                  {
                      logging.ClearProviders();
                      logging.SetMinimumLevel(LogLevel.Trace);
                  })
                  .UseNLog();
        }
    }
}
