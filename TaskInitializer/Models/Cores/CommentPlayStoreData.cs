using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    public class CommentPlayStoreData
    {
        [JsonProperty("packages")]
        public List<string> Packages { get; set; }
    }
}