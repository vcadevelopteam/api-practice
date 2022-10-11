using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class AlertMessageCron
    {
        [Column("emailalertmessage")]
        public string EmailAlertMessage { get; set; }

        [Column("emailalertsubject")]
        public string EmailAlertSubject { get; set; }

        [Column("supervisoremail")]
        public string SupervisorEmail { get; set; }

        [Column("variablecontext")]
        public string VariableContext { get; set; }

        [Column("emailalerttime")]
        public string EmailAlertTime { get; set; }

        [Column("supervisordesc")]
        public string SupervisorDesc { get; set; }

        [Column("asesoremail")]
        public string AsesorEmail { get; set; }

        [Column("channeltype")]
        public string ChannelType { get; set; }

        [Column("asesordesc")]
        public string AsesorDesc { get; set; }

        [Column("ticketnum")]
        public string TicketNum { get; set; }

        [Column("usergroup")]
        public string UserGroup { get; set; }

        [Column("channel")]
        public string Channel { get; set; }

        [Column("lasttag")]
        public string LastTag { get; set; }

        [Column("tags")]
        public string Tags { get; set; }

        [Column("conversationid")]
        public long? ConversationId { get; set; }

        [Column("supervisorid")]
        public long? SupervisorId { get; set; }

        [Column("channelid")]
        public long? ChannelId { get; set; }

        [Column("asesorid")]
        public long? AsesorId { get; set; }

        [Column("withresponse")]
        public bool? WithResponse { get; set; }
    }
}