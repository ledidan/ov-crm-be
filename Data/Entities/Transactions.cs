

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{

    public class Transactions
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int? PartnerId { get; set; }
        public Partner Partner { get; set; }
        public int PartnerLicenseId { get; set; }
        public PartnerLicense PartnerLicense { get; set; }
        public int ApplicationId { get; set; }
        public Application Application { get; set; }

        public int? ApplicationPlanId { get; set; }
        public ApplicationPlan ApplicationPlan { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "VND";

        public string PaymentMethod { get; set; } // vnpay, momo

        public string Status { get; set; } // pending, success, failed

        public string TransactionId { get; set; }

        [Column(TypeName = "json")]
        public string Metadata { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}