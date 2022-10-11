using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class CampaignData
    {
        [Column("last")]
        public bool Last { get; set; }
    }
}