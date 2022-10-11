using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class LinkedInToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("refresh_token_expires_in")]
        public long RefreshTokenExpiresIn { get; set; }
    }
}