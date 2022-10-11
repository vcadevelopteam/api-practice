using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class ReportSchedulerColumnJson
    {
        [JsonProperty("disabled")]
        public bool? Disabled { get; set; }

        [JsonProperty("checked")]
        public bool? Checked { get; set; }

        [JsonProperty("tablename")]
        public string Tablename { get; set; }

        [JsonProperty("columnname")]
        public string Columnname { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("join_table")]
        public string Join_table { get; set; }

        [JsonProperty("join_alias")]
        public string Join_alias { get; set; }

        [JsonProperty("join_on")]
        public string Join_on { get; set; }

        [JsonProperty("descriptionT")]
        public string DescriptionT { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }
    }
}