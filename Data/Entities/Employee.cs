namespace Data.Entities
{
    public class Employee : BaseEntity
    {
        public int Id { get; set; }
        public string? Fullname { get; set; }
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? StreetAddress { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public DateTime SignedContractDate { get; set; }
        public int PartnerId { get; set; }
        public virtual required Partner Partner { get; set; }
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<ContactEmployees> ContactEmployees { get; set; } = new List<ContactEmployees>();
        public ICollection<CustomerEmployees> CustomerEmployees { get; set; } = new List<CustomerEmployees>();
        public ICollection<InvoiceEmployees> InvoiceEmployees { get; set; } = new List<InvoiceEmployees>();
        public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();

    }
}
