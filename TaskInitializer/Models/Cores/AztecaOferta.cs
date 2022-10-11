using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class AztecaOferta
    {
        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("msjRequisitos")]
        public string MsjRequisitos { get; set; }

        [JsonProperty("offerMaxima")]
        public string OfferMaxima { get; set; }

        [JsonProperty("rate")]
        public string Rate { get; set; }

        [JsonProperty("dni")]
        public string Dni { get; set; }

        [JsonProperty("msj")]
        public string Msj { get; set; }

        [JsonProperty("idCampania")]
        public long IdCampania { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}