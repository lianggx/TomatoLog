using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace TomatoLog.Server.BLL
{
    public abstract class BaseConfigManager<T>
    {
        protected IConfiguration configuration;
        public BaseConfigManager(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.Load();
        }
        public T ConfigObject { get; set; }

        public T Load()
        {
            var json = System.IO.File.ReadAllText(ConfigFile);
            this.ConfigObject = JsonSerializer.Deserialize<T>(json);

            return ConfigObject;
        }

        public void Save()
        {
            var json = System.IO.File.ReadAllText(ConfigFile);
            System.IO.File.WriteAllText(ConfigFile, JsonSerializer.Serialize(this.ConfigObject));
        }

        public abstract string ConfigFile { get; }
    }
}
