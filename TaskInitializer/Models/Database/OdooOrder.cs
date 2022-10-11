using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class OdooOrder
    {
        [Column("conversationid")]
        public long? ConversationId { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}