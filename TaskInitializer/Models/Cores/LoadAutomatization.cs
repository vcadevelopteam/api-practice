using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    public class LoadAutomatization
    {
        [JsonProperty("parameters")]
        public List<LoadAutomatizationParameter> Parameters { get; set; }

        [JsonProperty("personjson")]
        public dynamic PersonJson { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("platformtype")]
        public string PlatformType { get; set; }

        [JsonProperty("shippingreason")]
        public string ShippingReason { get; set; }

        [JsonProperty("hsmtemplatename")]
        public string HsmTemplateName { get; set; }

        [JsonProperty("communicationchanneltype")]
        public string CommunicationChannelType { get; set; }

        [JsonProperty("hsmtemplateid")]
        public long HsmTemplateId { get; set; }

        [JsonProperty("communicationchannelid")]
        public long CommunicationChannelId { get; set; }
    }

    public class LoadAutomatizationParameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("variable")]
        public string Variable { get; set; }
    }

    public class LoadAutomatizationPersonJson
    {
        [JsonProperty("personid")]
        public long PersonId { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }
    }
}