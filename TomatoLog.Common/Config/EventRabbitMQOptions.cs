
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TomatoLog.Common.Config
{
    public class EventRabbitMQOptions : EventOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string vHost { get; set; }
        public string Exchange { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        public string RouteKey { get; set; }
    }
}
