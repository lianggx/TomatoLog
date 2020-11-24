﻿using Microsoft.Extensions.Logging;
using System;
using System.Text.Json.Serialization;

namespace TomatoLog.Common.Utilities
{
    public class LogMessage
    {
        public int EventId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectLabel { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel LogLevel { get; set; }
        public string Version { get; set; }
        public string IP { get; set; }
        public string[] IPList { get; set; }
        public string MachineName { get; set; }
        public string ThreadId { get; set; }
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }
        [JsonConverter(typeof(JsonConverterDateTimeParse))]
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public object Extra { get; set; }
    }
}
