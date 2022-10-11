using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("couponpromotion")]
    public class CouponPromotion
    {
        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Key]
        [Column("couponpromotionid")]
        public long CouponPromotionId { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("currentstock")]
        public long? CurrentStock { get; set; }

        [Column("maxstock")]
        public long? MaxStock { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("supplytimer")]
        public long? SupplyTimer { get; set; }

        [Column("bodega")]
        public string Bodega { get; set; }

        [Column("changeby")]
        public string ChangeBy { get; set; }

        [Column("createby")]
        public string CreateBy { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }
}