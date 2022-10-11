using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class MailConfig
    {
        [JsonProperty("MessageTemplateId")]
        public long MessageTemplateId { get; set; }

        [JsonProperty("CommunicationChannelSite")]
        public string CommunicationChannelSite { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("HsmTo")]
        public string HsmTo { get; set; }

        [JsonProperty("Origin")]
        public string Origin { get; set; }

        [JsonProperty("ShippingReason")]
        public string ShippingReason { get; set; }

        [JsonProperty("HsmId")]
        public string HsmId { get; set; }

        [JsonProperty("Body")]
        public string Body { get; set; }
    }
}