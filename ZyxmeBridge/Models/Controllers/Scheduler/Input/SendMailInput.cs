using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZyxMeBridge.Models.Controllers.Scheduler.Input
{
    public class SendMailInput
    {
        [JsonProperty("mailAttachmentData")]
        public byte[] MailAttachmentData { get; set; }

        [JsonProperty("attachments")]
        public List<MailAttachment> Attachments { get; set; }

        [JsonProperty("mailAttachmentName")]
        public string MailAttachmentName { get; set; }

        [JsonProperty("mailBlindAddress")]
        public string MailBlindAddress { get; set; }

        [JsonProperty("mailCopyAddress")]
        public string MailCopyAddress { get; set; }

        [JsonProperty("mailCredentials")]
        public string MailCredentials { get; set; }

        [JsonProperty("mailAddress")]
        public string MailAddress { get; set; }

        [JsonProperty("mailTitle")]
        public string MailTitle { get; set; }

        [JsonProperty("mailBody")]
        public string MailBody { get; set; }
    }

    public class MailCredentials
    {
        [JsonProperty("default_Credentials")]
        public bool DefaultCredentials { get; set; }

        [JsonProperty("enableSsl")]
        public bool EnableSsl { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }
    }
}