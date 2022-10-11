using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class WitaiSchedule
    {
        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("worker")]
        public long? Worker { get; set; }

        [Column("id")]
        public long? Id { get; set; }

        [Column("appid")]
        public string AppId { get; set; }

        [Column("token")]
        public string Token { get; set; }
    }
}