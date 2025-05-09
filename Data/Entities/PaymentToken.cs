using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class PaymentToken
    {
        public long Id { get; set; }
        [Required]
        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

        [Required]
        public int PartnerLicenseId { get; set; }
        public PartnerLicense PartnerLicense { get; set; }

        [Required]
        public string PaymentMethod { get; set; } // vnpay

        [Required]
        public string Token { get; set; }

        public string CardNumber { get; set; } // 4 số cuối

        public string ExpiryDate { get; set; } // MM/YYYY

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}