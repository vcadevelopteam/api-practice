using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class UploadFile
    {
        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}