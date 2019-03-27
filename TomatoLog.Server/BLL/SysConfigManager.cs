using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TomatoLog.Server.Models;

namespace TomatoLog.Server.BLL
{
    public class SysConfigManager
    {
        private IConfiguration configuration;
        public SysConfigManager(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.Load();
        }
        public ReportViewModel Report { get; private set; }

        public ReportViewModel Load()
        {
            var json = System.IO.File.ReadAllText(SysConfigFile);
            this.Report = JsonConvert.DeserializeObject<ReportViewModel>(json);

            return Report;
        }

        public ReportViewModel Save(ReportViewModel model)
        {
            var json = System.IO.File.ReadAllText(SysConfigFile);
            System.IO.File.WriteAllText(SysConfigFile, JsonConvert.SerializeObject(model));
            this.Report = model;
            return Report;
        }

        public string SysConfigFile
        {
            get
            {
                return this.configuration["TomatoLog:Config:SysConfig"];
            }
        }
    }
}
