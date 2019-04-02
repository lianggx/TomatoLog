using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomatoLog.Client.Extensions;
using TomatoLog.Client;
using TomatoLog.Common.Interface;
using Microsoft.Extensions.Logging;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private ITomatoLogClient logClient;
        public HomeController(ITomatoLogClient logClient)
        {
            this.logClient = logClient;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            try
            {
                await logClient.WriteLogAsync(1029, LogLevel.Warning, "Warning Infomation", "Warning Content", new { LastTime = DateTime.Now, Tips = "Warning" });
                throw new NotSupportedException("NotSupported Media Type");
            }
            catch (Exception ex)
            {
                await ex.AddTomatoLogAsync();
            }
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {

            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
