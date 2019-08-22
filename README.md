# TomatoLog
TomatoLog is a service which based on the .net Core platform.

### ![中文文档](https://github.com/lianggx/TomatoLog/blob/master/README_Zh-cn.md)

The TomatoLog is a middleware that includes both client and server.  So it is very easy to be used and deployed.

The client and server of TomatoLog are both built on the .net Core. Client logging data can be transferred through 3 kinds of streaming: Redis Streaming, RabbitMQ Streaming, and Kafka Streaming (which will be supported later). For the applications which not built-on .net core, you can transfer the logging data to the pipeline of Redis/RabbitMQ, by implementing the TomatoLog transferring protocol.

The TomatoLog Server can store the logging data into File, MongoDB, or Elastic Search. This can be set through the configuration file.  The TomatoLog Server provides a Web Console, which we can inquiry or search the logging data, or maintain the server filters/alter setting/notification setting.  The notifications can be sent through SMS or email.  Since the SMS messages are sent through HTTP request, so by using the different SMS HTTP request settings, we can send the notifications to the gateways which receiving messages through HTTP requests.

## TomatoLog System Architecture
![foundation](https://github.com/lianggx/pictures/blob/master/TomatoLog/system.png)



## Get Started

### Using the client side

Choose to install either of the following clients

``` C#
Install-Package TomatoLog.Client.Redis
Install-Package TomatoLog.Client.RabbitMQ
Install-Package TomatoLog.Client.Kafka
```

### Configure TomatoLogClient

```C#
        public void ConfigureServices(IServiceCollection services)
        {
			EventRabbitMQOptions options = new EventRabbitMQOptions
            {
                Logger = null,
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                ProjectLabel = "20272",
                ProjectName = "TomatoLog",
                SysOptions = new EventSysOptions
                {
                    EventId = true,
                    IP = true,
                    IPList = true,
                    MachineName = true,
                    ProcessId = true,
                    ProcessName = true,
                    ThreadId = true,
                    Timestamp = true,
                    UserName = true
                },
                Tags = null,
                Version = "1.0.1",
                Exchange = "TomatoLog-Exchange",
                ExchangeType = "direct",
                Host = "127.0.0.1",
                Password = "guest",
                Port = 5672,
                QueueName = "TomatoLog-Queue",
                RouteKey = "All",
                UserName = "guest",
                vHost = "TomatoLog"
            };

			services.AddSingleton<ITomatoLogClient, TomatoLogClientRabbitMQ>(factory =>
            {
                var client = new TomatoLogClientRabbitMQ(options);
                return client;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
```

### Using TomatoLogClient 

```C#
    public class HomeController : ControllerBase
    {
        private ITomatoLogClient logClient;
        public HomeController(ITomatoLogClient logClient)
        {
            this.logClient = logClient;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            try
            {
                await logClient.WriteLogAsync(1029, LogLevel.Warning, "Warning Infomation", "Warning Content", new { LastTime = DateTime.Now, Tips = "Warning" });
                throw new NotSupportedException("NotSupported Media Type");
            }
            catch (Exception ex)
            {
                await ex.AddTomatoLogAsync();
            }
            return new string[] { "value1", "value2" };
        }
	}
```

### Setup the server side

download the server-side compressed package file ![TomatoLog](https://github.com/lianggx/TomatoLog/releases/download/1.0.0/TomatoLog.zip)  (which only contains the project files)，You also need to install the .net core SDK 2.2+ in the hosting environment.

2) Extract the file, edit the appsetting.Environment.json file for the server side configurations, then deploy the configured server to your hosting environment. , TomatoLog can be hosted by IIS or other non-IIS Web Servers. The default port for the server is 20272.

### Editing server configuration file

```
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "TomatoLog": {
    "Cache-Redis": null, // the strategy for the distribution buffering policy for the filter
    "Config": {
      "SysConfig": "Config/SysConfig.json" // the system configuration file, which can be configured through Web Console
    },
    "Storage": {
      "Type": "ToFile", //ToFile/ToES/ToMongoDB 
      "File": "D:\\TomatoLog\\Storage", // If ToFile is selected, need to specify the absolution path at here
      "ES": "http://127.0.0.1:9200/", // if ToES is selected, need to specify the URL for the Elastic Search server at here 
      "MongoDB": "mongodb://root:root@127.0.0.1:27017/admin" // if ToMongoDB is selected, need to specify the connection string for MongoDB at here 
    },
    "Flow": {
      "Type": "RabbitMQ", // Redis/RabbitMQ/Kafaka  // the transferring protocol, which should be matched at both server & client
      "Redis": {
        "Connection": null,
        "Channel": "TomatoLogChannel"
      },
      "RabbitMQ": { // if using RibbitMQ at above, the node need to be configured
        "Host": "127.0.0.1",
        "Port": 5672,
        "UserName": "root",
        "Password": "123456",
        "vHost": "TomatoLog",
        "Exchange": "TomatoLog-Exchange",
        "ExchangeType": "direct",
        "QueueName": "TomatoLog-Queue",
        "RouteKey": "All",
        "Channels": 1 // the number of instances of message queue
      },
      "Kafka": {
        "Group": "TomatoLogServer",
        "BootstrapServers": "127.0.0.1:9092",
        "Topic": "TomatoLog"
      }
    }
  }
}

```
