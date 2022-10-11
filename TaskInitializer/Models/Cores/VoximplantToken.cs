using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class VoximplantAccessToken
    {
        [JsonProperty("result")]
        public VoximplantAccessTokenResult Result { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class VoximplantAccessTokenResult
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}