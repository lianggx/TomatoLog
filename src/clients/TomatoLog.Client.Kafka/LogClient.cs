using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatoLog.Common.Config;

namespace TomatoLog.Client.Kafka
{
    public class LogClient
    {
        private EventKafkaOptions options;
        private IProducer<Null, string> producer;

        public LogClient(EventKafkaOptions options)
        {
            this.options = options;
            var config = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers
            };

            producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task SendMessage(string message)
        {
            var msg = new Message<Null, string> { Value = message };
            await producer.ProduceAsync(options.Topic, msg);
        }
    }
}
