using TomatoLog.Common.Config;
using TomatoLog.Common.Interface;
using TomatoLog.Common.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TomatoLog.Common.Repository
{
    public abstract class LogWriter : ILogWriter
    {
        protected readonly StorageOptions options;
        protected readonly ILogger logger;

        protected LogWriter(StorageOptions options, ILogger log)
        {
            this.options = options;
            this.logger = log;
        }

        public abstract Task<int> Write(LogMessage message);

        public abstract Task<List<string>> GetProjects();

        public abstract Task<List<string>> GetLabels(string proj);

        public abstract Task<List<string>> List(string proj, string logLevel, string keyWrod = null, int page = 1, int pageSize = 100);
    }
}
