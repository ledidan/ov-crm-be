using Data.DTOs;

namespace Data.Responses
{
    public class TransactionMetadata
    {
        public List<int> LicenseIds { get; set; }
        public List<AppItem> AppItems { get; set; }
        public string BankCode { get; set; }
        public bool AutoRenew { get; set; }
        public string UniqueId { get; set; }
        public object TransferInfo { get; set; }
        public string CreateDate { get; set; }
    }
}
