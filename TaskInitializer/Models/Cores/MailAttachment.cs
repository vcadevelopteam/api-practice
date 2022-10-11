using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class MailAttachment
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}