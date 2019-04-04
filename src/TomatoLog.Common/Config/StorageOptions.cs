using TomatoLog.Common.Utilities;

namespace TomatoLog.Common.Config
{
    public class StorageOptions
    {
        public StorageType Type { get; set; }
        public string File { get; set; }
        public string ES { get; set; }
        public string MongoDB { get; set; }
        public string Others { get; set; }
    }
}
