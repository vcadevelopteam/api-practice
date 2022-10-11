using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    [Table("calllog")]
    public class CallLog
    {
        [Column("conversationtext")]
        public string ConversationText { get; set; }

        [Column("storagelink")]
        public string StorageLink { get; set; }

        [Column("changeby")]
        public string ChangeBy { get; set; }

        [Column("createby")]
        public string CreateBy { get; set; }

        [Column("duration")]
        public string Duration { get; set; }

        [Column("receptor")]
        public string Receptor { get; set; }

        [Column("emitter")]
        public string Emitter { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("size")]
        public string Size { get; set; }

        [Column("changedate")]
        public DateTime? ChangeDate { get; set; }

        [Column("createdate")]
        public DateTime? CreateDate { get; set; }

        [Key]
        [Column("calllogid")]
        public long CallLogId { get; set; }
    }
}