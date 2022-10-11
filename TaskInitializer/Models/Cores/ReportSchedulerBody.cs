using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    public class ReportSchedulerBody
    {
        [JsonProperty("columns")]
        public List<ReportSchedulerColumnJson> Columns { get; set; }

        [JsonProperty("filters")]
        public List<ReportSchedulerFilterJson> Filters { get; set; }

        [JsonProperty("parameters")]
        public ReportSchedulerParameters Parameters { get; set; }

        [JsonProperty("user")]
        public ReportSchedulerUser User { get; set; }

        [JsonProperty("summaries")]
        public dynamic Summaries { get; set; }
    }

    public class ReportSchedulerUser
    {
        [JsonProperty("corpid")]
        public long Corpid { get; set; }

        [JsonProperty("orgid")]
        public long Orgid { get; set; }

        [JsonProperty("userid")]
        public long Userid { get; set; }

        [JsonProperty("usr")]
        public string Usr { get; set; }
    }
}