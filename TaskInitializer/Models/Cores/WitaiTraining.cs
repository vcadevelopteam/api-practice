using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class WitaiTraining
    {
        [JsonProperty("training_status")]
        public string TrainingStatus { get; set; }
    }
}