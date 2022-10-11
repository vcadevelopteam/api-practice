using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("appstoretoken")]
    public class AppStoreToken
    {
        [Column("datetimeupdate")]
        public DateTime? DateTimeUpdated { get; set; }

        [Column("datetimecreate")]
        public DateTime? DateTimeCreate { get; set; }

        [Key]
        [Column("appstoretokenid")]
        public long AppStoreTokenId { get; set; }

        [Column("accesstoken")]
        public string AccessToken { get; set; }

        [Column("clientstate")]
        public string ClientState { get; set; }
    }
}