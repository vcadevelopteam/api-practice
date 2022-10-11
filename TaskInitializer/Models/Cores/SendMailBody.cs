using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    public class SendMailBody
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
}