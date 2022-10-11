using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ChannelSchedule
    {
        [Column("atencion")]
        public bool Atencion { get; set; }
    }
}