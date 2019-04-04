namespace TomatoLog.Common.Config
{
    public class EventRedisOptions : EventOptions
    {
        public string ConnectionString { get; set; }
        public string Channel { get; set; }
    }
}
