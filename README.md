# TomatoLog
TomatoLog for DotNETCore

The TomatoLog is a log middleware,It's easy to fast use for you. 

TomatoLog client and server are using for .NETCore,The clients provider two flowtype,Their is for Redis and RabbitMQ,You also develop other client to use. 

TomatoLog server also provider three method to log store,Their is Files/MongoDB/Elasticsearch,We also proider the web admin to manager.

## The TomatoLog system foundation
![foundation](https://github.com/lianggx/pictures/blob/master/TomatoLog/system.png)


##Get Started

### client

``` C#
Install-Package TomatoLog.Client.Redis
// or
Install-Package TomatoLog.Client.RabbitMQ
```

