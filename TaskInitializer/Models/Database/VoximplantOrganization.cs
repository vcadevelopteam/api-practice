using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class VoximplantOrganization
    {
        [Column("voximplantautomaticrecharge")]
        public bool? VoximplantAutomaticRecharge { get; set; }

        [Column("timezoneoffset")]
        public double? TimezoneOffset { get; set; }

        [Column("voximplantrechargepercentage")]
        public double? VoximplantRechargePercentage { get; set; }

        [Column("voximplantrechargefixed")]
        public double? VoximplantRechargeFixed { get; set; }

        [Column("voximplantrechargerundate")]
        public DateTime? VoximplantRechargeRunDate { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("voximplantrechargerange")]
        public long? VoximplantRechargeRange { get; set; }
    }
}