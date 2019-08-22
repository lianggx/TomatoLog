# TomatoLog
TomatoLog 是一个基于 .NETCore 平台的服务。

The TomatoLog 是一个中间件，包含客户端、服务端，非常容易使用和部署。

TomatoLog 的客户端和服务端目前都是基于 .NETCore 版本,客户端提供了三种日志流传输方式，目前实现了 Redis 和 RabbitMQ 流，剩余 Kafka 流式未实现。如果希望使用非 .NETCoer 平台的客户端，你可以自己开放其它第三方语言的客户端，通过实现 TomatoLog 传输协议，将数据传送到管道(Redis/RabbitMQ)中即可。

TomatoLog 服务端还提供了三种存储日志的方式，分别是 File、MongoDB、Elasticsearch，存储方式可以通过配置文件指定。在 TomatoLog 服务端，我们还提供了一个Web 控制台，通过该控制台，可以对日志进行查询、搜索，对服务过滤器进行配置，警报配置、通知发送等等，其中，可使用的警报通知方式有：SMS 和 Email 两种方式，但是，SMS 其本质是一个 Http 请求，通过 SMS 的配置，可以实现向所有提供了 Http 接口的网关发送通知。

## TomatoLog 系统架构
![foundation](https://github.com/lianggx/pictures/blob/master/TomatoLog/system.png)



## Get Started

### 使用客户端

选择安装以下客户端中的任意一项

``` C#
Install-Package TomatoLog.Client.Redis
Install-Package TomatoLog.Client.RabbitMQ
Install-Package TomatoLog.Client.Kafka
```

### 配置客户端

```
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

### 使用 TomatoLogClient 

```
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

### 部署服务端

首先，下载服务端压缩包文件 ![TomatoLog](https://github.com/lianggx/TomatoLog/releases/download/1.0.0/TomatoLog.zip) ，该压缩包仅包含项目运行必需文件，托管该服务端的服务器上必须按照 DotNET Core SDK 2.2+

接下来，解压文件，修改 appsetting.Environment.json 文件将服务器进行配置，将配置好的服务端部署到你的服务器上，可以为 TomatoLog 选择 IIS 或者其它托管方式，服务端默认运行端口为：20272.

### 编辑服务端配置文件

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
    "Cache-Redis": null, // 过滤器会使用该分布式缓存进行策略考量，如果有配置
    "Config": {
      "SysConfig": "Config/SysConfig.json" // 系统配置文件，可通过Web控制台进行配置
    },
    "Storage": {
      "Type": "ToFile", //ToFile/ToES/ToMongoDB 可以选择的存储方式
      "File": "D:\\TomatoLog\\Storage", // 如果Type选择了 ToFile ，则这里必须指定绝对路径
      "ES": "http://127.0.0.1:9200/", // 如果Type选择了ToES，这里必须配置 Elasticsearch 服务地址
      "MongoDB": "mongodb://root:root@127.0.0.1:27017/admin" //如果Type选择了ToMongoDB，这里必须配置 ToMongoDB 数据库链接
    },
    "Flow": {
      "Type": "RabbitMQ", // Redis/RabbitMQ/Kafaka 这里指定客户端和服务器的传输管道类型，两端配置必须一致
      "Redis": {
        "Connection": null,
        "Channel": "TomatoLogChannel"
      },
      "RabbitMQ": { // 如果使用了 RabbitMQ，则必须配置该节点
        "Host": "127.0.0.1",
        "Port": 5672,
        "UserName": "root",
        "Password": "123456",
        "vHost": "TomatoLog",
        "Exchange": "TomatoLog-Exchange",
        "ExchangeType": "direct",
        "QueueName": "TomatoLog-Queue",
        "RouteKey": "All",
        "Channels": 1 // 运行的消息队列实例数量
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
