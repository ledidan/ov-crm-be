

namespace Data.DTOs
{
    public class AppPaymentRequest
    {
        public int PartnerLicenseId { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string LicenceType { get; set; }
        public string PaymentMethod { get; set; }
        public string BankCode { get; set; }// NCB, VISA, VNPAYQR
    }
}