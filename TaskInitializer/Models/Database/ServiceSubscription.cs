using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("servicesubscription")]
    public class ServiceSubscription
    {
        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Column("subscriptiondatestart")]
        public DateTime? SubscriptionDateStart { get; set; }

        [Column("subscriptiondateend")]
        public DateTime? SubscriptionDateEnd { get; set; }

        [Column("account")]
        public string Account { get; set; }

        [Column("node")]
        public string Node { get; set; }

        [Column("historycurrent")]
        public string HistoryCurrent { get; set; }

        [Column("historyprevious")]
        public string HistoryPrevious { get; set; }

        [Column("extradata")]
        public string ExtraData { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("createby")]
        public string CreateBy { get; set; }

        [Column("changeby")]
        public string ChangeBy { get; set; }

        [Column("webhook")]
        public string Webhook { get; set; }

        [Key]
        [Column("servicesubscriptionid")]
        public long ServiceSubscriptionId { get; set; }

        [Column("interval")]
        public long? Interval { get; set; }

        [Column("longdesc")]
        public long? LongDesc { get; set; }
    }
}