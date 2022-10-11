using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class WitaiConfig
    {
        [Column("id")]
        public long? Id { get; set; }
    }
}