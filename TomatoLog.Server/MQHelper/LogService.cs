using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using TomatoLog.Common.Interface;
using TomatoLog.Common.Utilities;
using TomatoLog.Server.BLL;

namespace TomatoLog.Server.MQHelper
{
    public class LogService : MQServiceBase
    {
        private ILogWriter logWriter;
        private FilterService filterService;
        public LogService(ILogWriter logWriter, FilterService filterService, MQSetting setting, ILogger logger) : base(setting, logger)
        {
            this.logWriter = logWriter;
            this.filterService = filterService;
            base.vHost = setting.vHost;
            base.Exchange = setting.Exchange;
            base.Binds.Add(new BindInfo()
            {
                ExchangeType = setting.ExchangeType,
                Queue = setting.QueueName,
                RouterKey = setting.RouteKey,
                OnReceived = this.SmsLog_OnReceived
            });
        }

        /// <summary>
        ///  消息
        /// </summary>
        /// <param name="message"></param>
        public void SmsLog_OnReceived(MessageBody message)
        {
            if (message.Error)
            {
                logger.LogError(new EventId(1001), message.Exception, message.ErrorMessage);
                return;
            }
            try
            {
                var log = JsonConvert.DeserializeObject<LogMessage>(message.Content);
                logWriter.Write(log);
                filterService.Filter(log);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "|" + message.Content, ex);
            }
            message.Consumer.Model.BasicAck(message.BasicDeliver.DeliveryTag, true);

        }
    }
}
