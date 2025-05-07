


namespace Data.DTOs
{
    public class PartnerLicenseDTO
    {
        public int Id { get; set; }           // Primary Key}
        public int PartnerId { get; set; }
        public int ApplicationId { get; set; } // Primary Key

        public int? ApplicationPlanId { get; set; }

        public DateTime StartDate { get; set; }  // Ngày bắt đầu sử dụng ứng dụng
        public DateTime EndDate { get; set; }    // Ngày hết hạn licence của ứng dụng
        public string LicenceType { get; set; }   // FreeTrial, Monthly, Yearly, Lifetime
        public decimal? CustomPrice { get; set; }
        public bool AutoRenew { get; set; } = false;
        public int? MaxEmployeesExpected { get; set; }
        public string Status { get; set; }       // Active, Expired, Suspended, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRenewedAt { get; set; }
    }
}