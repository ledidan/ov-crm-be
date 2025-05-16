

namespace Data.DTOs
{
    public class AppPaymentRequest
    {
        public int UserId { get; set; }
        public int? PartnerLicenseId { get; set; }

        public int? PartnerId { get; set; }
        public List<AppItem> AppItems { get; set; } = new List<AppItem>();
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";

        public string PaymentMethod { get; set; } // vnpay, momo
        //  VNBANK    Thẻ ATM - Tài khoản ngân hàng nội địa
        //  INTCARD   Thẻ thanh toán quốc tế
        public string? BankCode { get; set; } // VNBANK, INTCARD, VNPAYQR, TRANSFER

    }

    public class AppItem
    {   
        public int ApplicationPlanId { get; set; }

        public string ApplicationName { get; set; } // Tên ứng dụng
        public string LicenceType { get; set; } // Pro, FreeTrial, Enterprise, Lifetime

        public bool AutoRenew { get; set; } = false;

        public int? Duration { get; set; } // Số tháng (Monthly) hoặc năm (Yearly)
    }
}