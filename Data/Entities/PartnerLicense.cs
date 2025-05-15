


using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class PartnerLicense
    {
        public int Id { get; set; }           // Primary Key}

        public int UserId { get; set; }
        public int? PartnerId { get; set; }
        public Partner Partner { get; set; }  // Navigation property

        public int ApplicationId { get; set; } // Primary Key
        public Application Application { get; set; } // Navigation property

        public int? ApplicationPlanId { get; set; }
        public ApplicationPlan ApplicationPlan { get; set; }

        public DateTime StartDate { get; set; }  // Ngày bắt đầu sử dụng ứng dụng
        public DateTime EndDate { get; set; }    // Ngày hết hạn licence của ứng dụng
        public string? LicenceType { get; set; } = string.Empty;  // FreeTrial, Monthly, Yearly, Lifetime
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CustomPrice { get; set; }
        public bool AutoRenew { get; set; } = false;
        public int? MaxEmployeesExpected { get; set; } = 0; // Số lượng nhân viên tối đa dự kiến sử dụng ứng dụng
        public string? Status { get; set; } = string.Empty;  // Active, Expired, Pending.
        public string? ActivationCode { get; set; } = string.Empty;// Mã kích hoạt ứng dụng
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRenewedAt { get; set; }
    }

}