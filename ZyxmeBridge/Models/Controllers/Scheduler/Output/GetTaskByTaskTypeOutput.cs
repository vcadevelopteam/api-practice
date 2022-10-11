using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZyxMeBridge.Models.Controllers.Scheduler.Output
{
    public class GetTaskByTaskTypeOutput
    {
        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}