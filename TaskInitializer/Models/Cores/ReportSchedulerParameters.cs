using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    public class ReportSchedulerParameters
    {
        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("headerClient")]
        public List<ReportSchedulerParametersHeader> HeaderClient { get; set; }

        [JsonProperty("formatToExport")]
        public string FormatToExport { get; set; }

        [JsonProperty("reportName")]
        public string ReportName { get; set; }
    }

    public class ReportSchedulerParametersHeader
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }
    }
}