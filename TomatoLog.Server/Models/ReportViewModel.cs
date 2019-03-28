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
        public SmsModel Sms { get; set; }
        public EmailModel Email { get; set; }
    }

    public class SettingModel
    {
        public string ProjectName { get; set; }
        public bool On { get; set; }
        public int Time { get; set; }
        public int Count { get; set; }
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
