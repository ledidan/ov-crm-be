using Data.Enums;

namespace Data.Entities
{
    public class Employee : BaseEntity
    {
        public int Id { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? OfficePhone { get; set; }
        public string? OfficeEmail { get; set; }
        public string? TaxIdentificationNumber { get; set; }

        public JobStatus JobStatus { get; set; }
        public DateTime? SignedProbationaryContract { get; set; }

        public DateTime? Resignation { get; set; }
        public DateTime SignedContractDate { get; set; }
        public int PartnerId { get; set; }

        public int? JobPositionGroupId { get; set; }
        public int? JobTitleGroupId { get; set; }
        public virtual required Partner Partner { get; set; }

        public List<Contact> Contacts { get; set; } = new List<Contact>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public List<Product> Products { get; set; } = new List<Product>();

        public List<Order> Orders { get; set; } = new List<Order>();

        public ICollection<ContactEmployees> ContactEmployees { get; set; } = new List<ContactEmployees>();
        public ICollection<CustomerEmployees> CustomerEmployees { get; set; } = new List<CustomerEmployees>();
        public ICollection<InvoiceEmployees> InvoiceEmployees { get; set; } = new List<InvoiceEmployees>();
        public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();
        public ICollection<ProductEmployees> ProductEmployees { get; set; } = new List<ProductEmployees>();

        public ICollection<OrderEmployees> OrderEmployees { get; set; } = new List<OrderEmployees>();

    }
}
