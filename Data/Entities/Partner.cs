namespace Data.Entities
{
    public class Partner : BaseEntity
    {
        public int Id { get; set; }
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? EmailContact { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TotalEmployees { get; set; }
        public bool? IsOrganization { get; set; }
        public string? OwnerFullName { get; set; }

        public ICollection<JobTitleGroup> JobTitleGroup { get; set; }
        public ICollection<JobPositionGroup> JobPositionGroup { get; set; }
        public ICollection<ContactEmployees> ContactEmployees { get; set; } = new List<ContactEmployees>();
        public ICollection<CustomerEmployees> CustomerEmployees { get; set; } = new List<CustomerEmployees>();
        public ICollection<InvoiceEmployees> InvoiceEmployees { get; set; } = new List<InvoiceEmployees>();
        public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();
        public ICollection<ProductEmployees> ProductEmployees { get; set; } = new List<ProductEmployees>();
        public ICollection<OrderContacts> OrderContacts { get; set; } = new List<OrderContacts>();
        public ICollection<CustomerContacts> CustomerContacts { get; set; } = new List<CustomerContacts>();
        public ICollection<CustomerOrders> CustomerOrders { get; set; } = new List<CustomerOrders>();


    }
}
