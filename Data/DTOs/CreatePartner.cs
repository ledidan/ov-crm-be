using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CreatePartner
    {
        public required string ShortName { get; set; }

        [Required]
        public required string Name { get; set; }
        [Required]
        public string? TaxIdentificationNumber { get; set; }
        public string? LogoUrl { get; set; }
        [Required]
        public string? EmailContact { get; set; }
        public string? PhoneNumber { get; set; }

        public string? TotalEmployees { get; set; }
        [Required]
        public bool? IsOrganization { get; set; }
        public string? OwnerFullName { get; set; }

    }
}
