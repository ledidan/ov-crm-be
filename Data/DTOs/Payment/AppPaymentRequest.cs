

namespace Data.DTOs
{
    public class AppPaymentRequest
    {
        public int PartnerLicenseId { get; set; }

        public decimal? Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string LicenceType { get; set; } // Pro, FreeTrial, Enterprise, Lifetime
        public string PaymentMethod { get; set; } // vnpay, momo
        // vnp_BankCode=  VNBANK    Thẻ ATM - Tài khoản ngân hàng nội địa
        // vnp_BankCode=  INTCARD   Thẻ thanh toán quốc tế
        public string BankCode { get; set; } // VNBANK, INTCARD, VNPAYQR

    }
}