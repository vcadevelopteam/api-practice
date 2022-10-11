using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("servicetoken")]
    public class ServiceToken
    {
        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Column("refreshtoken")]
        public string RefreshToken { get; set; }

        [Column("accesstoken")]
        public string AccessToken { get; set; }

        [Column("extradata")]
        public string ExtraData { get; set; }

        [Column("changeby")]
        public string ChangeBy { get; set; }

        [Column("createby")]
        public string CreateBy { get; set; }

        [Column("account")]
        public string Account { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Key]
        [Column("servicetokenid")]
        public long ServiceTokenId { get; set; }

        [Column("interval")]
        public long? Interval { get; set; }

        [Column("longdesc")]
        public long? LongDesc { get; set; }
    }
}