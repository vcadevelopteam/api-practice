using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class CallInteraction
    {
        [Column("interactiontext")]
        public string InteractionText { get; set; }

        [Column("conversationid")]
        public long ConversationId { get; set; }

        [Column("interactionid")]
        public long InteractionId { get; set; }

        [Column("personid")]
        public long PersonId { get; set; }

        [Column("corpid")]
        public long CorpId { get; set; }

        [Column("orgid")]
        public long OrgId { get; set; }
    }
}