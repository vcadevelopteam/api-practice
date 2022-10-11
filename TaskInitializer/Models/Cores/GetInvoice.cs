using Newtonsoft.Json;

namespace TaskInitializer.Models.Cores
{
    public class GetInvoiceOutput
    {
        [JsonProperty("result")]
        public GetInvoiceOutputResult Result { get; set; }

        [JsonProperty("operationMessage")]
        public string OperationMessage { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class GetInvoiceOutputResult
    {
        [JsonProperty("cadenaCodigoQr")]
        public string CadenaCodigoQr { get; set; }

        [JsonProperty("codigoHash")]
        public string CodigoHash { get; set; }

        [JsonProperty("correlativoCpe")]
        public string CorrelativoCpe { get; set; }

        [JsonProperty("estadoDocumento")]
        public string EstadoDocumento { get; set; }

        [JsonProperty("serieCpe")]
        public string SerieCpe { get; set; }

        [JsonProperty("sunat_description")]
        public string SunatDescription { get; set; }

        [JsonProperty("sunatNote")]
        public string SunatNote { get; set; }

        [JsonProperty("sunatResponseCode")]
        public string SunatResponseCode { get; set; }

        [JsonProperty("ticketSunat")]
        public string TicketSunat { get; set; }

        [JsonProperty("tipoCpe")]
        public string TipoCpe { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("urlCdrSunat")]
        public string UrlCdrSunat { get; set; }

        [JsonProperty("urlPdf")]
        public string UrlPdf { get; set; }

        [JsonProperty("urlXml")]
        public string UrlXml { get; set; }
    }
}