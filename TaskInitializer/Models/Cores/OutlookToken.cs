using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class OutlookToken
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}