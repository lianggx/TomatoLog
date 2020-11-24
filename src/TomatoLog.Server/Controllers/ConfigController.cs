using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TomatoLog.Server.BLL;
using TomatoLog.Server.Models;

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
            var model = sysConfigManager.ConfigObject;
            return View(model);
        }

        [HttpPost("Setting")]
        public IActionResult Setting([FromForm] SettingModel model)
        {
            sysConfigManager.ConfigObject.Setting = model;
            return Save();
        }

        [HttpPost("Sms")]
        public IActionResult Sms([FromForm] SmsModel model)
        {
            sysConfigManager.ConfigObject.Sms = model;
            return Save();
        }

        [HttpPost("Email")]
        public IActionResult Email([FromForm] EmailModel model)
        {
            sysConfigManager.ConfigObject.Email = model;
            return Save();
        }

        private IActionResult Save()
        {
            sysConfigManager.Save();
            TempData["Success"] = true;
            return RedirectToAction("Index");
        }
    }
}
