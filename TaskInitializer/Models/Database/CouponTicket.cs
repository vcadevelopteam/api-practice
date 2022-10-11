using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("couponticket")]
    public class CouponTicket
    {
        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Key]
        [Column("couponticketid")]
        public long CouponTicketId { get; set; }

        [Column("conversationid")]
        public long? ConversationId { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("coupongrocerid")]
        public long? CouponGrocerId { get; set; }

        [Column("couponshopid")]
        public long? CouponShopId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("changeby")]
        public string ChangeBy { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("createby")]
        public string CreateBy { get; set; }

        [Column("number")]
        public string Number { get; set; }

        [Column("promotion")]
        public string Promotion { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }
}