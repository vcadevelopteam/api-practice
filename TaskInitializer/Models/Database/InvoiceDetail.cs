using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("invoicedetail")]
    public class InvoiceDetail
    {
        [Key]
        [Column("invoicedetailid")]
        public long InvoiceDetailId { get; set; }

        [Column("invoiceid")]
        public long? InvoiceId { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("quantity")]
        public long? Quantity { get; set; }

        [Column("totaligv")]
        public double? TotalIgv { get; set; }

        [Column("totalamount")]
        public double? TotalAmount { get; set; }

        [Column("igvrate")]
        public double? IgvRate { get; set; }

        [Column("productprice")]
        public double? ProductPrice { get; set; }

        [Column("productnetprice")]
        public double? ProductnetPrice { get; set; }

        [Column("productnetworth")]
        public double? ProductNetWorth { get; set; }

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

        [Column("productcode")]
        public string ProductCode { get; set; }

        [Column("hasigv")]
        public string HasIgv { get; set; }

        [Column("saletype")]
        public string SaleType { get; set; }

        [Column("igvtribute")]
        public string IgvTribute { get; set; }

        [Column("measureunit")]
        public string MeasureUnit { get; set; }

        [Column("productdescription")]
        public string ProductDescription { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }
    }
}