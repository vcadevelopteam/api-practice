using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ReportSchedulerData
    {
        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("reportschedulerid")]
        public long? ReportSchedulerId { get; set; }

        [Column("userid")]
        public long? UserId { get; set; }

        [Column("datajson")]
        public string DataJson { get; set; }

        [Column("enddate")]
        public string EndDate { get; set; }

        [Column("filterjson")]
        public string FilterJson { get; set; }

        [Column("mailbody")]
        public string MailBody { get; set; }

        [Column("mailbodyobject")]
        public string MailBodyObject { get; set; }

        [Column("mailcc")]
        public string MailCc { get; set; }

        [Column("mailsubject")]
        public string MailSubject { get; set; }

        [Column("mailto")]
        public string MailTo { get; set; }

        [Column("startdate")]
        public string StartDate { get; set; }

        [Column("origin")]
        public string Origin { get; set; }

        [Column("originType")]
        public string OriginType { get; set; }

        [Column("reportName")]
        public string ReportName { get; set; }

        [Column("timezoneoffset")]
        public string TimeZoneOffset { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("usr")]
        public string Usr { get; set; }
    }
}