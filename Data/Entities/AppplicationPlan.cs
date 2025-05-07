


using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ApplicationPlan
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Name { get; set; } // Basic, Pro, Enterprise
        public string Description { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceMonthly { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceYearly { get; set; }
        public int MaxEmployees { get; set; }

        public ICollection<PartnerLicense> PartnerLicenses { get; set; }

    }
}