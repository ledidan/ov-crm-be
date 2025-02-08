


namespace Data.DTOs
{
    public class PartnerDTO
    {
        public int Id { get; set; }
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? EmailContact { get; set; }
        public string? PhoneNumber { get; set; }
        public int? OwnerId { get; set; }
    }
}