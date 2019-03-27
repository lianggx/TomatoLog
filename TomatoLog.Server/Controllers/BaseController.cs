using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
