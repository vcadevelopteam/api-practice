using Newtonsoft.Json;

namespace ZyxMeBridge.Models.Controllers.Scheduler.Input
{
    public class GetTaskByTaskIdInput
    {
        [JsonProperty("taskSchedulerId")]
        public long? TaskSchedulerId { get; set; }
    }
}