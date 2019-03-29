using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using TomatoLog.Common.Utilities;

namespace TomatoLog.Common.Config
{
    public abstract class EventOptions
    {
        private string projectName = string.Empty;
        public virtual string ProjectName
        {
            get => projectName;
            set
            {
                Check.NotNull(value, "projectName");
                if (value.IndexOf('.') > 0 || value.IndexOf(' ') > 0)
                    throw new ArgumentOutOfRangeException("The project name doesn't contains \".\"  or space");
                projectName = value;
            }
        }
        public virtual string ProjectLabel { get; set; }
        public virtual Hashtable Tags { get; set; }
        public virtual ILogger Logger { get; set; }
        public virtual EventSysOptions SysOptions { get; set; }
        public virtual string Version { get; set; }
        public virtual LogLevel LogLevel { get; set; }
    }
}
