using TomatoLog.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TomatoLog.Common.Interface
{
    public interface ILogWriter
    {
        Task<int> Write(LogMessage message);
        Task<List<string>> GetProjects();

        Task<List<string>> GetLabels(string proj);

        Task<List<string>> List(string proj, string logLevel, string keyWrod = null, int page = 1, int pageSize = 100);
    }
}
