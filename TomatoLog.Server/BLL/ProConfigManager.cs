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
    public class ProConfigManager : BaseConfigManager<List<ReportViewModel>>
    {
        public ProConfigManager(IConfiguration configuration) : base(configuration) { }

        public override string ConfigFile
        {
            get
            {
                return this.configuration["TomatoLog:Config:ProConfig"];
            }
        }
    }
}
