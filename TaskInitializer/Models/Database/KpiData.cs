using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class KpiData
    {
        [Column("tasklastrundate")]
        public DateTime? TaskLastRunDate { get; set; }

        [Column("taskinterval")]
        public double? TaskInterval { get; set; }

        [Column("corpid")]
        public long? CorpId { get; set; }

        [Column("kpiid")]
        public long? KpiId { get; set; }

        [Column("orgid")]
        public long? OrgId { get; set; }

        [Column("kpiname")]
        public string KpiName { get; set; }

        [Column("taskperiod")]
        public string TaskPeriod { get; set; }
    }
}