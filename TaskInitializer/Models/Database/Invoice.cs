using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("invoice")]
    public class Invoice
    {
        [Column("sendtosunat")]
        public bool? SendToSunat { get; set; }

        [Column("returnpdf")]
        public bool? ReturnPdf { get; set; }

        [Column("returnxmlsunat")]
        public bool? ReturnXmlSunat { get; set; }

        [Column("returnxml")]
        public bool? ReturnXml { get; set; }

        [Column("subtotal")]
        public double? Subtotal { get; set; }

        [Column("taxes")]
        public double? Taxes { get; set; }

        [Column("totalamount")]
        public double? TotalAmount { get; set; }

        [Column("exchangerate")]
        public double? ExchangeRate { get; set; }

        [Key]
        [Column("invoiceid")]
        public long InvoiceId { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("year")]
        public long? Year { get; set; }

        [Column("month")]
        public long? Month { get; set; }

        [Column("correlative")]
        public long? Correlative { get; set; }

        [Column("filenumber")]
        public long? FileNumber { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("createby")]
        public string CreateBy { get; set; }

        [Column("changeby")]
        public string ChangeBy { get; set; }

        [Column("issuerruc")]
        public string IssuerRuc { get; set; }

        [Column("issuerbusinessname")]
        public string IssuerBusinessName { get; set; }

        [Column("issuertradename")]
        public string IssuerTradeName { get; set; }

        [Column("issuerfiscaladdress")]
        public string IssuerFiscalAddress { get; set; }

        [Column("issuerubigeo")]
        public string IssuerUbigeo { get; set; }

        [Column("emittertype")]
        public string EmitterType { get; set; }

        [Column("annexcode")]
        public string AnnexCode { get; set; }

        [Column("printingformat")]
        public string PrintingFormat { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("sunaturl")]
        public string SunatUrl { get; set; }

        [Column("sunatusername")]
        public string SunatUsername { get; set; }

        [Column("xmlversion")]
        public string XmlVersion { get; set; }

        [Column("ublversion")]
        public string UblVersion { get; set; }

        [Column("receiverdoctype")]
        public string ReceiverDocType { get; set; }

        [Column("receiverdocnum")]
        public string ReceiverDocNum { get; set; }

        [Column("receiverbusinessname")]
        public string ReceiverBusinessName { get; set; }

        [Column("receiverfiscaladdress")]
        public string ReceiverFiscalAddress { get; set; }

        [Column("receivercountry")]
        public string ReceiverCountry { get; set; }

        [Column("receivermail")]
        public string ReceiverMail { get; set; }

        [Column("invoicetype")]
        public string InvoiceType { get; set; }

        [Column("sunatopecode")]
        public string SunatOpeCode { get; set; }

        [Column("serie")]
        public string Serie { get; set; }

        [Column("concept")]
        public string Concept { get; set; }

        [Column("currency")]
        public string Currency { get; set; }

        [Column("invoicestatus")]
        public string InvoiceStatus { get; set; }

        [Column("errordescription")]
        public string ErrorDescription { get; set; }

        [Column("qrcode")]
        public string QrCode { get; set; }

        [Column("hashcode")]
        public string HashCode { get; set; }

        [Column("urlcdr")]
        public string UrlCdr { get; set; }

        [Column("urlpdf")]
        public string UrlPdf { get; set; }

        [Column("urlxml")]
        public string UrlXml { get; set; }

        [Column("purchaseorder")]
        public string PurchaseOrder { get; set; }

        [Column("executingunitcode")]
        public string ExecutingUnitCode { get; set; }

        [Column("selectionprocessnumber")]
        public string SelectionProcessNumber { get; set; }

        [Column("contractnumber")]
        public string ContractNumber { get; set; }

        [Column("comments")]
        public string Comments { get; set; }

        [Column("orderid")]
        public string OrderId { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("invoicedate")]
        public DateTime? InvoiceDate { get; set; }

        [Column("expirationdate")]
        public DateTime? ExpirationDate { get; set; }
    }
}