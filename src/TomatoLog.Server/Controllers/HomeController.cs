using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TomatoLog.Common.Interface;
using TomatoLog.Common.Utilities;
using TomatoLog.Server.Models;
using TomatoLog.Server.ViewModels;

namespace TomatoLog.Server.Controllers
{
    public class HomeController : BaseController
    {
        public static string[] FIELDS = new string[] { "EventId", "Timestamp", "IP", "IPList", "ErrorMessage", "ProcessName", "ProjectName" };
        private ILogWriter logWriter;
        private HttpClient client;
        private IConfiguration cfg;
        public HomeController(ILogWriter logWriter, HttpClient client, IConfiguration cfg, ILogger<HomeController> logger) : base(cfg, logger)
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

        public async Task<IActionResult> Labels([FromQuery] string proj)
        {
            ViewBag.Proj = proj;
            var labels = await logWriter.GetLabels(proj);
            List<FileDesc> list = new List<FileDesc>();
            labels.ForEach(s =>
            {
                var obj = JsonSerializer.Deserialize<FileDesc>(s);
                list.Add(obj);
            });
            return View(list);
        }

        public async Task<IActionResult> Detail([FromQuery] MessageViewModel model)
        {
            ViewBag.Message = model;
            ViewBag.FIELDS = FIELDS;
            var result = await logWriter.List(model.Project, model.Label, model.Keyword, model.Page, model.PageSize);
            return View(result);
        }

        [HttpPost]
        public IActionResult Detail([FromForm] string[] fields, [FromForm] MessageViewModel model)
        {
            FIELDS = fields;
            return RedirectToAction("Detail", model);
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
