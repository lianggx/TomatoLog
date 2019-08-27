using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomatoLog.Client.Extensions;
using TomatoLog.Common.Interface;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ITomatoLogClient logClient;
        private readonly ILogger logger;
        public HomeController(ILogger<HomeController> logger, ITomatoLogClient logClient)
        {
            this.logger = logger;
            this.logClient = logClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            // Used by ILogger
            this.logger.LogError("测试出错了");

            // Used By ITomatoLogClient
            try
            {
                await this.logClient.WriteLogAsync(1029, LogLevel.Warning, "Warning Infomation", "Warning Content", new { LastTime = DateTime.Now, Tips = "Warning" });
                throw new NotSupportedException("NotSupported Media Type");
            }
            catch (Exception ex)
            {
                await ex.AddTomatoLogAsync();
            }

            return new string[] { "value1", "value2" };
        }
    }
}
