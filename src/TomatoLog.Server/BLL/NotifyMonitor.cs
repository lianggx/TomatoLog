using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using TomatoLog.Common.Utilities;
using TomatoLog.Server.Models;

namespace TomatoLog.Server.BLL
{
    public class NotifyMonitor
    {
        private Regex regex = new Regex(@"{[^}]+}");
        private IConfiguration cfg;
        private HttpClient httpClient;
        private ILogger log;
        private ReportViewModel report;

        public NotifyMonitor(HttpClient httpClient, IConfiguration cfg, ILogger log, ReportViewModel report)
        {
            Check.NotNull(report, nameof(ReportViewModel));

            this.httpClient = httpClient;
            this.cfg = cfg;
            this.log = log;
            this.report = report;
        }

        public async void SendSms(LogMessage msg)
        {
            if (report.Sms != null && report.Sms.On)
            {
                try
                {
                    var data = CreateContent(msg, report.Sms.Content);
                    HttpMethod method = HttpMethod.Get;
                    switch (report.Sms.Method.ToUpper())
                    {
                        case "POST": method = HttpMethod.Post; break;
                        case "GET": method = HttpMethod.Get; break;
                        default: throw new NotSupportedException(nameof(HttpMethod));
                    }
                    var res = await HttpHelper.HttpRequest(httpClient, report.Sms.Url, method, data, report.Sms.ContentType);
                    log?.LogDebug($"SendSms:{report.Sms.Url} | {data} | {res}");
                }
                catch (Exception ex)
                {
                    log.LogError("{0} {1}", ex.Message, ex.StackTrace);
                }
            }
        }

        public async void SendEmail(LogMessage msg)
        {
            var email = report.Email;
            if (email != null && email.On)
            {
                try
                {
                    var title = CreateContent(msg, email.Title);
                    var data = CreateContent(msg, email.Content);
                    MailMessage message = new MailMessage(email.UserName, email.Receiver, title, data)
                    {
                        IsBodyHtml = true,
                        SubjectEncoding = Encoding.UTF8,
                        BodyEncoding = Encoding.UTF8,
                        Priority = MailPriority.High
                    };

                    if (email.CC != null)
                    {
                        foreach (var c in email.CC.Split(";"))
                        {
                            message.CC.Add(c);
                        }
                    }


                    SmtpClient smtpClient = new SmtpClient(email.Host, email.Port)
                    {
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Credentials = new System.Net.NetworkCredential(email.UserName, email.Password),
                        EnableSsl = email.SSL
                    };

                    smtpClient.SendCompleted += (sender, e) =>
                    {
                        if (e.Error != null)
                            log.LogError(e.Error.Message, e.Error);
                    };

                    await smtpClient.SendMailAsync(message);
                    smtpClient.Dispose();

                    log?.LogDebug($"SendEmail:{email.Host} | {email.Port} | {email.Receiver} | {email.CC} | {title} | {data}");
                }
                catch (Exception ex)
                {
                    log.LogError("{0} {1}", ex.Message, ex.StackTrace);
                }
            }
        }

        private string CreateContent(LogMessage msg, string content)
        {
            var ht = JToken.FromObject(msg);

            var matchs = regex.Matches(content);
            for (int i = 0; i < matchs.Count; i++)
            {
                var key = matchs[i].Value.Replace("{", "").Replace("}", "");
                var field = ht[key];
                if (field != null)
                {
                    var value = field.Value<string>();
                    content = content.Replace(matchs[i].Value, value);
                }
            }

            return content;
        }
    }
}
