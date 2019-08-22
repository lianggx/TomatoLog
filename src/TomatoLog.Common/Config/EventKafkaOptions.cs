namespace TomatoLog.Common.Config
{
    public class EventKafkaOptions : EventOptions
    {
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }
    }
}
