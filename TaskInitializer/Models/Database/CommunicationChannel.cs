using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("communicationchannel")]
    public class CommunicationChannel
    {
        [Column("communicationchannelsite")]
        public string CommunicationChannelSite { get; set; }

        [Column("schedule")]
        public string Schedule { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Key]
        [Column("communicationchannelid")]
        public long CommunicationChannelId { get; set; }

        [Column("corpid")]
        public long CorpId { get; set; }

        [Column("orgid")]
        public long OrgId { get; set; }

        [Column("channelactive")]
        public bool? ChannelActive { get; set; }
    }
}