using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class BridgeResponse
    {
        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}