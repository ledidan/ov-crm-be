namespace Data.Entities
{
    public class Partner : BaseEntity
    {
        public int Id { get; set; }
        public string? CompanyCode { get; set; } = string.Empty;

        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? EmailContact { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TotalEmployees { get; set; }
        public bool? IsOrganization { get; set; }
        public string? OwnerFullName { get; set; }
        public string? BusinessRegistrationNumber { get; set; } = string.Empty;

        public DateTime? EstablishedDate { get; set; } = DateTime.Now;
        public string? Address { get; set; } = string.Empty;

        public string? Fax { get; set; } = string.Empty;

        public string? Website { get; set; } = string.Empty;

        public bool? IsInitialized { get; set; } = false;

        public DateTime? InitializedAt { get; set; } = DateTime.Now;

        public ICollection<PartnerLicense> PartnerLicenses { get; set; } = new List<PartnerLicense>();
        public ICollection<JobTitleGroup> JobTitleGroup { get; set; } = new List<JobTitleGroup>();
        public ICollection<JobPositionGroup> JobPositionGroup { get; set; } = new List<JobPositionGroup>();
        public ICollection<ContactEmployees> ContactEmployees { get; set; } = new List<ContactEmployees>();
        public ICollection<CustomerEmployees> CustomerEmployees { get; set; } = new List<CustomerEmployees>();
        public ICollection<InvoiceEmployees> InvoiceEmployees { get; set; } = new List<InvoiceEmployees>();
        public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();
        public ICollection<ProductEmployees> ProductEmployees { get; set; } = new List<ProductEmployees>();
        public ICollection<OrderContacts> OrderContacts { get; set; } = new List<OrderContacts>();
        public ICollection<CustomerContacts> CustomerContacts { get; set; } = new List<CustomerContacts>();

        public ICollection<OpportunityContacts> OpportunityContacts { get; set; } = new List<OpportunityContacts>();


    }
}
