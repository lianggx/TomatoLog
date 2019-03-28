using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TomatoLog.Server.Models;

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
            this.ConfigObject = JsonConvert.DeserializeObject<T>(json);

            return ConfigObject;
        }

        public void Save()
        {
            var json = System.IO.File.ReadAllText(ConfigFile);
            System.IO.File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(this.ConfigObject));
        }

        public abstract string ConfigFile { get; }
    }
}
