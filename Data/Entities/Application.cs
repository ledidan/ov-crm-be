


namespace Data.Entities
{
    public class Application : BaseEntity
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }

        public string? Description { get; set; }
        public ICollection<PartnerLicense> PartnerLicenses { get; set; }
    }
}