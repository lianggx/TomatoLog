using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TomatoLog.Server.ViewModels
{
    public class ProjectConfigViewModel
    {
        [Required(ErrorMessage = "The parameter projectName must be required!")] public string Setting_ProjectName { get; set; }
        public bool Setting_On { get; set; }
        public int Setting_Time { get; set; }
        public int Setting_Count { get; set; }
        public string Setting_Levels { get; set; }


        public bool Sms_On { get; set; }
        public string Sms_Url { get; set; }
        public string Sms_Method { get; set; }
        public string Sms_Content { get; set; }
        public string Sms_ContentType { get; set; }


        public bool Email_On { get; set; }
        public string Email_Host { get; set; }
        public int Email_Port { get; set; }
        public string Email_UserName { get; set; }
        public string Email_Password { get; set; }
        public string Email_Receiver { get; set; }
        public string Email_CC { get; set; }
        public string Email_Title { get; set; }
        public string Email_Content { get; set; }
        public bool Email_SSL { get; set; }
    }
}
