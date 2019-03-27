using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TomatoLog.Common.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TomatoLog.Server.BLL;
using TomatoLog.Server.Models;

namespace TomatoLog.Server.Controllers
{
    public class HomeController : BaseController
    {
        public static string[] FIELDS = new string[] { "EventId", "Timestamp", "IP", "IPList", "ErrorMessage", "ProcessName", "ProjectName" };
        private ILogWriter logWriter;
        private HttpClient client;
        private IConfiguration cfg;
        public HomeController(ILogWriter logWriter, HttpClient client, IConfiguration cfg, ILogger<LogController> logger) : base(cfg, logger)
        {
            this.logWriter = logWriter;
            this.client = client;
            this.cfg = cfg;
        }

        public async Task<IActionResult> Index()
        {
            var projs = await logWriter.GetProjects();
            ViewBag.Projects = projs;
            return View();
        }

        public async Task<IActionResult> Detail([FromQuery]string proj, [FromQuery]string label, [FromQuery]string keyword)
        {
            ViewBag.Proj = proj;
            ViewBag.Label = label;
            ViewBag.FIELDS = FIELDS;
            ViewBag.Keyword = keyword;
            var result = await logWriter.List(proj, label, keyword);
            return View(result);
        }


        [HttpPost]
        public IActionResult Detail([FromForm]string[] fields, [FromForm]string proj, [FromForm]string label, [FromForm]string keyword)
        {
            FIELDS = fields;
            return RedirectToAction("Detail", new { proj, label, keyword });
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
