using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class SecurityValidation
    {
        [Column("passwordchangedate")]
        public DateTime? PasswordChangeDate { get; set; }

        [Column("lastactivitydate")]
        public DateTime? LastActivityDate { get; set; }

        [Column("now")]
        public DateTime? Now { get; set; }

        [Column("periodvaliditypwd")]
        public long? PeriodValidityPwd { get; set; }

        [Column("maxinactivedays")]
        public long? MaxInactiveDays { get; set; }

        [Column("pwdvaliddays")]
        public long? PwdValidDays { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("userid")]
        public long? UserId { get; set; }

        [Column("fullname")]
        public string FullName { get; set; }

        [Column("action")]
        public string Action { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("usr")]
        public string Usr { get; set; }
    }
}