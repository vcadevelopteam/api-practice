using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("taskscheduler")]
    public class TaskData
    {
        [Column("datetimeoriginalstart")]
        public DateTime? DateTimeOriginalStart { get; set; }

        [Column("datetimelastrun")]
        public DateTime? DateTimeLastRun { get; set; }

        [Column("datetimestart")]
        public DateTime? DateTimeStart { get; set; }

        [Column("datetimeend")]
        public DateTime? DateTimeEnd { get; set; }

        [Column("repeatinterval")]
        public double? RepeatInterval { get; set; }

        [Column("taskprocessedids")]
        public string TaskProcessedIds { get; set; }

        [Column("taskbody")]
        public string TaskBody { get; set; }

        [Column("tasktype")]
        public string TaskType { get; set; }

        [Key]
        [Column("taskschedulerid")]
        public long TaskSchedulerId { get; set; }

        [Column("repeatmode")]
        public long? RepeatMode { get; set; }

        [Column("repeatflag")]
        public bool? RepeatFlag { get; set; }

        [Column("completed")]
        public bool? Completed { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }
    }
}