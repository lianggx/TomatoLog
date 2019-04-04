using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TomatoLog.Common.Config;

namespace TomatoLog.Common.Interface
{
    public interface ITomatoLogClient
    {
        Task WriteLogAsync(int eventId, LogLevel logLevel, string message, string content = null, object extra = null);

        EventOptions Options { get; set; }
    }
}
