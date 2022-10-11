using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class SessionData
    {
        [Column("corpid")]
        public long CorpId { get; set; }

        [Column("userid")]
        public long UserId { get; set; }

        [Column("orgid")]
        public long OrgId { get; set; }
    }
}