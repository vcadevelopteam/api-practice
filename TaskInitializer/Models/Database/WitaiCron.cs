using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class WitaiCron
    {
        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("id")]
        public long? Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("lang")]
        public string Lang { get; set; }

        [Column("timezone")]
        public string Timezone { get; set; }
    }
}