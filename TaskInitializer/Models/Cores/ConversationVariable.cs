using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class ConversationVariable
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}