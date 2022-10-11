using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("integration_cmbrando_cmbrando_bodegas")]
    public class CouponLaraigoShop
    {
        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("bodega")]
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