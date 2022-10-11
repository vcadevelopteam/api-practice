using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("paymentpending")]
    public class PaymentPending
    {
        [Key]
        [Column("paymentpendingid")]
        public long PaymentPendingId { get; set; }

        [Column("attempt")]
        public long Attempt { get; set; }

        [Column("corpid")]
        public long CorpId { get; set; }

        [Column("orgid")]
        public long OrgId { get; set; }

        [Column("invoiceid")]
        public long InvoiceId { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("lastbody")]
        public string LastBody { get; set; }

        [Column("lastresponse")]
        public string LastResponse { get; set; }

        [Column("createby")]
        public string CreateBy { get; set; }

        [Column("changeby")]
        public string ChangeBy { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("rundate")]
        public DateTime? RunDate { get; set; }
    }
}