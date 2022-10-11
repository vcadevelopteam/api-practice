using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    internal class Schedule
    {
        [JsonProperty("days")]
        public List<string> Days { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }
}