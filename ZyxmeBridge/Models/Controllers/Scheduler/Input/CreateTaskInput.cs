using Newtonsoft.Json;

namespace ZyxMeBridge.Models.Controllers.Scheduler.Input
{
    public class CreateTaskInput
    {
        [JsonProperty("taskRepeatInterval")]
        public string TaskRepeatInterval { get; set; }

        [JsonProperty("taskConversation")]
        public string TaskConversation { get; set; }

        [JsonProperty("taskOrganization")]
        public string TaskOrganization { get; set; }

        [JsonProperty("taskCorporation")]
        public string TaskCorporation { get; set; }

        [JsonProperty("taskRepeatFlag")]
        public string TaskRepeatFlag { get; set; }

        [JsonProperty("taskRepeatMode")]
        public string TaskRepeatMode { get; set; }

        [JsonProperty("taskStartDate")]
        public string TaskStartDate { get; set; }

        [JsonProperty("taskEndDate")]
        public string TaskEndDate { get; set; }

        [JsonProperty("taskType")]
        public string TaskType { get; set; }

        [JsonProperty("taskBody")]
        public dynamic TaskBody { get; set; }
    }
}