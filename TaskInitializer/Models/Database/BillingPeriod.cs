using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class BillingPeriod
    {
        [Column("p_bill")]
        public bool? Bill { get; set; }

        [Column("p_updated")]
        public bool? Updated { get; set; }
    }
}