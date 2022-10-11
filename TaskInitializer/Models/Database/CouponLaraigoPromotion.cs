using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("integration_cmbrando_cmbrando_promociones")]
    public class CouponLaraigoPromotion
    {
        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("bodega")]
        public string CouponShop { get; set; }

        [Column("promocion")]
        public string Name { get; set; }

        [Column("stockactual")]
        public string CurrentStock { get; set; }

        [Column("limitestock")]
        public string MaxStock { get; set; }

        [Column("tiempostock")]
        public string SupplyTimer { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }
    }
}