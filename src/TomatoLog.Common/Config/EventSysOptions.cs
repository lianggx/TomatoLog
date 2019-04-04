namespace TomatoLog.Common.Config
{
    public class EventSysOptions
    {
        public bool EventId { get; set; }
        public bool IP { get; set; }
        public bool IPList { get; set; }
        public bool MachineName { get; set; }
        public bool ThreadId { get; set; }
        public bool ProcessId { get; set; }
        public bool ProcessName { get; set; }
        public bool Timestamp { get; set; }
        public bool UserName { get; set; }
        public bool ErrorMessage { get; set; } = true;
        public bool StackTrace { get; set; } = true;
    }
}
