using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class WitaiResponse
    {
        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}