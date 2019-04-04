using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TomatoLog.Server.Controllers
{
    public class BaseController : Controller
    {
        protected IConfiguration configuration;
        private ILogger logger;
        public BaseController(IConfiguration configuration, ILogger logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }
    }
}
