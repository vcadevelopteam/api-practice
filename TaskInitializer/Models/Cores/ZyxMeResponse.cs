using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class ZyxMeResponse
    {
        [JsonProperty("result")]
        public dynamic Result { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }
}