using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TomatoLog.Common.Interface;

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
