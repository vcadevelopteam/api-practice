using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ConversationData
    {
        [Column("communicationchanneldescription")]
        public string CommunicationChannelDescription { get; set; }

        [Column("communicationchanneltype")]
        public string CommunicationChannelType { get; set; }

        [Column("organization")]
        public string Organization { get; set; }

        [Column("corporation")]
        public string Corporation { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("conversationnumber")]
        public long ConversationNumber { get; set; }

        [Column("startdate")]
        public DateTime StartDate { get; set; }
    }
}