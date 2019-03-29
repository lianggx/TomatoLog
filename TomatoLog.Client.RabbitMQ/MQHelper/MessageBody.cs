using RabbitMQ.Client.Events;
using System;

namespace TomatoLog.Client.RabbitMQ.MQHelper
{
    public class MessageBody
    {
        public EventingBasicConsumer Consumer { get; set; }
        public BasicDeliverEventArgs BasicDeliver { get; set; }
        /// <summary>
        ///  0=Success
        /// </summary>
        public int Code { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public bool Error { get; set; }
        public Exception Exception { get; set; }
    }
}
