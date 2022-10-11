using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class ReportSchedulerFilterJson
    {
        [JsonProperty("columnname")]
        public string Columnname { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("type_filter")]
        public string TypeFilter { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("join_alias")]
        public string Join_alias { get; set; }

        [JsonProperty("join_on")]
        public string Join_on { get; set; }

        [JsonProperty("join_table")]
        public string Join_table { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }
}