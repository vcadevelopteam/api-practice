using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("couponshop")]
    public class CouponShop
    {
        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Key]
        [Column("couponshopid")]
        public long CouponShopId { get; set; }

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