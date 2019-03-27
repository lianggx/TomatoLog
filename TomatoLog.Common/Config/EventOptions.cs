using TomatoLog.Common.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
                    throw new ArgumentOutOfRangeException("项目名称不允许试用符号 . 或者空格");
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
