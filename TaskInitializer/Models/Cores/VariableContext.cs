using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class VariableContext
    {
        [JsonProperty("columnType")]
        public string ColumnType { get; set; }

        [JsonProperty("fieldName")]
        public string FieldName { get; set; }

        [JsonProperty("tableName")]
        public string TableName { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("persistence")]
        public bool? Persistence { get; set; }
    }
}