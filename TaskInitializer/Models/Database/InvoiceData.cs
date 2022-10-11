using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class InvoiceData
    {
        [Column("p_automaticpayment")]
        public bool? AutomaticPayment { get; set; }

        [Column("p_invoiceid")]
        public long InvoiceId { get; set; }
    }
}