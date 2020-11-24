using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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
