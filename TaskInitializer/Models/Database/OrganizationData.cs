using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class OrganizationData
    {
        [Column("corpdescription")]
        public string CorpDescription { get; set; }

        [Column("orgdescription")]
        public string OrgDescription { get; set; }

        [Column("corpid")]
        public long CorpId { get; set; }

        [Column("orgid")]
        public long OrgId { get; set; }
    }
}