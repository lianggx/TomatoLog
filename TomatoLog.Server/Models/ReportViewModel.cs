using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TomatoLog.Server.Models
{
    public class ReportViewModel
    {
        public SettingModel Setting { get; set; }
        /// <summary>
        ///  短信警报配置
        /// </summary>
        public SmsModel Sms { get; set; }

        /// <summary>
        ///  邮件警报配置
        /// </summary>
        public EmailModel Email { get; set; }
    }

    public class SettingModel
    {
        public bool On { get; set; }
        /// <summary>
        ///  警报周期，以S为单位
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// 发生 N 次后发送一次警报
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        ///  LogLevel
        /// </summary>
        public string Levels { get; set; }
    }

    public class SmsModel
    {
        public bool On { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    public class EmailModel
    {
        public bool On { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Receiver { get; set; }
        public string CC { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool SSL { get; set; }
    }
}
