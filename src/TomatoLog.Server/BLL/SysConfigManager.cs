using Microsoft.Extensions.Configuration;
using TomatoLog.Server.Models;

namespace TomatoLog.Server.BLL
{
    public class SysConfigManager : BaseConfigManager<ReportViewModel>
    {
        public SysConfigManager(IConfiguration configuration) : base(configuration) { }

        public override string ConfigFile
        {
            get
            {
                return this.configuration["TomatoLog:Config:SysConfig"];
            }
        }
    }
}
