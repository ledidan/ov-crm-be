


namespace Data.DTOs
{
    public class PartnerDTO
    {
        public int Id { get; set; }
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public string? TaxIdentificationNumber { get; set; }

        public string? TotalEmployees { get; set; }

        public bool? IsOrganization { get; set; }
        public string? OwnerFullName { get; set; }

        public string? LogoUrl { get; set; }
        public string? EmailContact { get; set; }
        public string? PhoneNumber { get; set; }

         public bool? IsInitialized { get; set; } = false;

        public DateTime? InitializedAt { get; set; } = DateTime.Now;
    }
}