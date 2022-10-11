using Newtonsoft.Json;

namespace ZyxMeBridge.Models.Controllers.Scheduler.Input
{
    public class GetTaskByTaskTypeInput
    {
        [JsonProperty("taskType")]
        public string TaskType { get; set; }
    }
}