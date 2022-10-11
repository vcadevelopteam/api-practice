using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("sessionlog")]
    public class SessionLog
    {
        [Column("activenumber")]
        public long? ActiveNumber { get; set; }

        [Column("idlenumber")]
        public long? IdleNumber { get; set; }

        [Column("datetime")]
        public DateTime? DateTime { get; set; }

        [Column("columndata")]
        public string ColumnData { get; set; }

        [Key]
        [Column("sessionlogid")]
        public long SessionLogId { get; set; }
    }
}