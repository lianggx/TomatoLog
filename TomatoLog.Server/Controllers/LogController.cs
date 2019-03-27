using TomatoLog.Common.Interface;
using TomatoLog.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomatoLog.Server.Controllers
{
    [Route("log")]
    public class LogController : BaseController
    {
        private ILogWriter logWriter;
        public LogController(ILogWriter logWriter, IConfiguration cfg, ILogger<LogController> logger) : base(cfg, logger)
        {
            this.logWriter = logWriter;
        }

        [HttpPost("labels")]
        public async Task<IActionResult> GetLabels(string proj)
        {
            var labels = await logWriter.GetLabels(proj);

            return Json(labels);
        }
    }
}
