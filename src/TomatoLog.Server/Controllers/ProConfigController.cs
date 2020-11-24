using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using TomatoLog.Server.BLL;
using TomatoLog.Server.Models;
using TomatoLog.Server.ViewModels;

namespace TomatoLog.Server.Controllers
{
    [Route("proconfig")]
    public class ProConfigController : BaseController
    {
        private ProConfigManager proManager;
        public ProConfigController(ProConfigManager proManager, IConfiguration cfg, ILogger<ConfigController> logger) : base(cfg, logger)
        {
            this.proManager = proManager;
            if (proManager.ConfigObject == null)
                proManager.ConfigObject = new List<ReportViewModel>();
        }

        public IActionResult Index()
        {
            var model = proManager.ConfigObject;
            return View(model);
        }

        [HttpGet("Detail")]
        public IActionResult Detail([FromQuery] string projectName)
        {
            ViewBag.IsValid = ControllerContext.ModelState.IsValid;


            ProjectConfigViewModel model = new ProjectConfigViewModel();
            if (!string.IsNullOrEmpty(projectName))
            {
                var report = proManager.ConfigObject.FirstOrDefault(f => f.Setting.ProjectName == projectName);
                if (report != null)
                    model = new ProjectConfigViewModel
                    {
                        Email_CC = report.Email.CC,
                        Email_Content = report.Email.Content,
                        Email_Host = report.Email.Host,
                        Email_On = report.Email.On,
                        Email_Password = report.Email.Password,
                        Email_Port = report.Email.Port,
                        Email_Receiver = report.Email.Receiver,
                        Email_SSL = report.Email.SSL,
                        Email_Title = report.Email.Title,
                        Email_UserName = report.Email.UserName,

                        Setting_Count = report.Setting.Count,
                        Setting_Levels = report.Setting.Levels,
                        Setting_On = report.Setting.On,
                        Setting_ProjectName = report.Setting.ProjectName,
                        Setting_Time = report.Setting.Time,

                        Sms_Content = report.Sms.Content,
                        Sms_ContentType = report.Sms.ContentType,
                        Sms_Method = report.Sms.Method,
                        Sms_On = report.Sms.On,
                        Sms_Url = report.Sms.Url
                    };
            };
            return View(model);
        }

        [HttpPost("Detail")]
        public IActionResult Detail([FromForm] ProjectConfigViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Setting_ProjectName))
            {
                var project = proManager.ConfigObject.FirstOrDefault(f => f.Setting.ProjectName.ToLower() == model.Setting_ProjectName.ToLower());
                if (project == null)
                {
                    project = new ReportViewModel
                    {
                        Email = new EmailModel
                        {
                            CC = model.Email_CC,
                            Content = model.Email_Content,
                            Host = model.Email_Host,
                            On = model.Email_On,
                            Password = model.Email_Password,
                            Port = model.Email_Port,
                            Receiver = model.Email_Receiver,
                            SSL = model.Email_SSL,
                            Title = model.Email_Title,
                            UserName = model.Email_UserName
                        },

                        Setting = new SettingModel
                        {
                            Count = model.Setting_Count,
                            Levels = model.Setting_Levels,
                            On = model.Setting_On,
                            ProjectName = model.Setting_ProjectName,
                            Time = model.Setting_Time
                        },
                        Sms = new SmsModel
                        {
                            Content = model.Sms_Content,
                            ContentType = model.Sms_ContentType,
                            Method = model.Sms_Method,
                            On = model.Sms_On,
                            Url = model.Sms_Url
                        }
                    };
                    proManager.ConfigObject.Add(project);
                }
                else
                {
                    project.Email.CC = model.Email_CC;
                    project.Email.Content = model.Email_Content;
                    project.Email.Host = model.Email_Host;
                    project.Email.On = model.Email_On;
                    project.Email.Password = model.Email_Password;
                    project.Email.Port = model.Email_Port;
                    project.Email.Receiver = model.Email_Receiver;
                    project.Email.SSL = model.Email_SSL;
                    project.Email.Title = model.Email_Title;
                    project.Email.UserName = model.Email_UserName;

                    project.Setting.Count = model.Setting_Count;
                    project.Setting.Levels = model.Setting_Levels;
                    project.Setting.On = model.Setting_On;
                    project.Setting.ProjectName = model.Setting_ProjectName;
                    project.Setting.Time = model.Setting_Time;

                    project.Sms.Content = model.Sms_Content;
                    project.Sms.ContentType = model.Sms_ContentType;
                    project.Sms.Method = model.Sms_Method;
                    project.Sms.On = model.Sms_On;
                    project.Sms.Url = model.Sms_Url;
                }

                proManager.Save();
                TempData["Success"] = true;
            }
            ViewBag.IsValid = ControllerContext.ModelState.IsValid;
            return View(model);
        }

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] ProjectViewModel model)
        {
            if (string.IsNullOrEmpty(model.ProjectName))
            {
                return Json(new { Code = 401, Message = "The parameter ProjectName must be required!" });
            }

            var project = proManager.ConfigObject.FirstOrDefault(f => f.Setting.ProjectName.ToLower() == model.ProjectName.ToLower());
            if (project == null)
            {
                return Json(new { Code = 404, Message = $"The projectName {model.ProjectName} was not found!" });
            }

            proManager.ConfigObject.Remove(project);

            return Json(new { Code = 0, Message = "Success" });
        }
    }
}
