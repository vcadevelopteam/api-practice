using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class SessionInformation
    {
        [Column("query_start")]
        public DateTime? QueryStart { get; set; }

        [Column("datname")]
        public string DatName { get; set; }

        [Column("usename")]
        public string UseName { get; set; }

        [Column("query")]
        public string Query { get; set; }

        [Column("state")]
        public string State { get; set; }

        [Column("pid")]
        public long Pid { get; set; }
    }
}