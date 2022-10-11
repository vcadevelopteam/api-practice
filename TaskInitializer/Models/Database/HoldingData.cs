using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class HoldingData
    {
        [Column("countanswered")]
        public long CountAnswered { get; set; }

        [Column("countwaiting")]
        public long CountWaiting { get; set; }

        [Column("suspendidos")]
        public long Suspendidos { get; set; }

        [Column("cerrados")]
        public long Cerrados { get; set; }

        [Column("activos")]
        public long Activos { get; set; }

        [Column("userid")]
        public long UserId { get; set; }

        [Column("displayname")]
        public string DisplayName { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("icono")]
        public string Icono { get; set; }
    }
}