using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class LaraigoConversation
    {
        [Column("communicationchannelid")]
        public long? CommunicationChannelId { get; set; }

        [Column("personid")]
        public long? PersonId { get; set; }
    }
}