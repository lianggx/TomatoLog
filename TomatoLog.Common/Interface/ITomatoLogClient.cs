using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatoLog.Common.Config;
using Microsoft.Extensions.Logging;

namespace TomatoLog.Common.Interface
{
    public interface ITomatoLogClient
    {
        Task WriteLogAsync(int eventId, LogLevel logLevel, string message, string content = null, object extra = null);

        EventOptions Options { get; set; }
    }
}
