using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class AbandonedTicket
    {
        [Column("personcommunicationchannel")]
        public string PersonCommunicationChannel { get; set; }

        [Column("interactiontype")]
        public string InteractionType { get; set; }

        [Column("messagetext")]
        public string MessageText { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("communicationchannel")]
        public long CommunicationChannel { get; set; }

        [Column("lastinteractionid")]
        public long LastInteractionId { get; set; }

        [Column("conversationid")]
        public long ConversationId { get; set; }

        [Column("personid")]
        public long PersonId { get; set; }

        [Column("corpid")]
        public long CorpId { get; set; }

        [Column("orgid")]
        public long OrgId { get; set; }

        [Column("closeticket")]
        public bool CloseTicket { get; set; }
    }
}