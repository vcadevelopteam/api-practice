using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class AztecaPaperless
    {
        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("dni")]
        public string Dni { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}