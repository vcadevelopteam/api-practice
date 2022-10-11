namespace TaskInitializer.Models.Cores
{
    public class ReceiptData
    {
        public long CorpId { get; set; }
        public long Month { get; set; }
        public long OrgId { get; set; }
        public long Year { get; set; }

        public string ReceiptType { get; set; }
    }
}