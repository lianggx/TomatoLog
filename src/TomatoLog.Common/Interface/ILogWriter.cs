using System.Collections.Generic;
using System.Threading.Tasks;
using TomatoLog.Common.Utilities;

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
