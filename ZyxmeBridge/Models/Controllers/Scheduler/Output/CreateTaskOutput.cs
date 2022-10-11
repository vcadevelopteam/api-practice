using Newtonsoft.Json;

namespace ZyxMeBridge.Models.Controllers.Scheduler.Output
{
    public class CreateTaskOutput
    {
        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}