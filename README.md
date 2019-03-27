# TomatoLog
TomatoLog 是一个基于 .NETCore 平台的服务。

The TomatoLog 是一个中间件，包含客户端、服务端，非常容易使用和部署。

TomatoLog 的客户端和服务端目前都是基于 .NETCore 版本,客户端提供了三种日志流传输方式，目前实现了 Redis 和 RabbitMQ 流，剩余 Kafka 流式未实现。如果希望使用非 .NETCoer 平台的客户端，你可以自己开放其它第三方语言的客户端，通过实现 TomatoLog 传输协议，将数据传送到管道(Redis/RabbitMQ)中即可。

TomatoLog 服务端还提供了三种存储日志的方式，分别是 File、MongoDB、Elasticsearch，存储方式可以通过配置文件指定。在 TomatoLog 服务端，我们还提供了一个Web 控制台，通过该控制台，可以对日志进行查询、搜索，对服务过滤器进行配置，警报配置、通知发送等等，其中，可使用的警报通知方式有：SMS 和 Email 两种方式，但是，SMS 其本质是一个 Http 请求，通过 SMS 的配置，可以实现向所有提供了 Http 接口的网关发送通知。

## TomatoLog 系统架构
![foundation](https://github.com/lianggx/pictures/blob/master/TomatoLog/system.png)



## Get Started

### 使用客户端

选择安装以下两种客户端中的任意一项

``` C#
Install-Package TomatoLog.Client.Redis
Install-Package TomatoLog.Client.RabbitMQ
```

