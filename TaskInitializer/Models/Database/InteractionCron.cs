using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class InteractionCron
    {
        [Column("interactionid")]
        public long? InteractionId { get; set; }

        [Column("ticketnum")]
        public string TicketNum { get; set; }

        [Column("interactiondate")]
        public string InteractionDate { get; set; }

        [Column("interactiontime")]
        public string InteractionTime { get; set; }

        [Column("interactionuser")]
        public string InteractionUser { get; set; }

        [Column("interactiontype")]
        public string InteractionType { get; set; }

        [Column("interactiontext")]
        public string InteractionText { get; set; }
    }
}