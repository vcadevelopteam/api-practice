using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class AttachmentData
    {
        [Column("interaction_text")]
        public string InteractionText { get; set; }

        [Column("interaction_type")]
        public string InteractionType { get; set; }

        [Column("ticketnum")]
        public string TicketNum { get; set; }
    }
}