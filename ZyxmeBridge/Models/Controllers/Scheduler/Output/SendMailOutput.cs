using Newtonsoft.Json;

namespace ZyxMeBridge.Models.Controllers.Scheduler.Output
{
    public class SendMailOutput
    {
        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}