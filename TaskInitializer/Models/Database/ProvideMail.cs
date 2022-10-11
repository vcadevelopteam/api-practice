using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ProviderMail
    {
        [Column("id")]
        public long? Id { get; set; }

        [Column("email_notification")]
        public string EmailNotification { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}