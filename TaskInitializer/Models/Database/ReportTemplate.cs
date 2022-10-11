using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ReportTemplate
    {
        [Column("columnjson")]
        public string ColumnJson { get; set; }
    }
}