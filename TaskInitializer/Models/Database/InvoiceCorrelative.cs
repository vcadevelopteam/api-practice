using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class InvoiceCorrelative
    {
        [Column("p_correlative")]
        public long Correlative { get; set; }
    }
}