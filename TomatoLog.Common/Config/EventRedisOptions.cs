using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TomatoLog.Common.Config
{
    public class EventRedisOptions : EventOptions
    {
        public string ConnectionString { get; set; }
        public string Channel { get; set; }
    }
}
