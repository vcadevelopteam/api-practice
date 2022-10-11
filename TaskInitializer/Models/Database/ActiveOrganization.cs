using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ActiveOrganization
    {
        [Column("corpid")]
        public long CorpId { get; set; }

        [Column("orgid")]
        public long OrgId { get; set; }
    }
}