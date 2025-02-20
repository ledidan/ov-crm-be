using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CreatePartner
    {
        [Required]
        public required string ShortName { get; set; }

        [Required]
        public required string Name { get; set; }

        public string? TaxIdentificationNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? EmailContact { get; set; }
        public string? PhoneNumber { get; set; }

        public int? OwnerId { get; set; }
    }
}
