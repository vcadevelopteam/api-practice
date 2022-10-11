using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class ReportSchedulerDataJson
    {
        [JsonProperty("corpid")]
        public long CorpId { get; set; }

        [JsonProperty("orgid")]
        public long OrgId { get; set; }

        [JsonProperty("reporttemplateid")]
        public long ReportTemplateId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("columnjson")]
        public string ColumnJson { get; set; }

        [JsonProperty("filterjson")]
        public string FilterJson { get; set; }

        [JsonProperty("summaryjson")]
        public string SummaryJson { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }
    }
}