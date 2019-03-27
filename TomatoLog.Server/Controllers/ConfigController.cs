using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections;
using TomatoLog.Server.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TomatoLog.Server.BLL;

namespace TomatoLog.Server.Controllers
{
    [Route("config")]
    public class ConfigController : BaseController
    {
        private SysConfigManager sysConfigManager;
        public ConfigController(SysConfigManager sysConfigManager, IConfiguration cfg, ILogger<ConfigController> logger) : base(cfg, logger)
        {
            this.sysConfigManager = sysConfigManager;
        }

        public IActionResult Index()
        {
            var model = sysConfigManager.Report;
            return View(model);
        }

        [HttpPost("Setting")]
        public IActionResult Setting([FromForm]SettingModel model)
        {
            sysConfigManager.Report.Setting = model;
            return Save();
        }

        [HttpPost("Sms")]
        public IActionResult Sms([FromForm]SmsModel model)
        {
            sysConfigManager.Report.Sms = model;
            return Save();
        }

        [HttpPost("Email")]
        public IActionResult Email([FromForm]EmailModel model)
        {
            sysConfigManager.Report.Email = model;
            return Save();
        }

        private IActionResult Save()
        {
            var report = sysConfigManager.Save(sysConfigManager.Report);
            TempData["Succees"] = true;
            return RedirectToAction("Index");
        }
    }
}
