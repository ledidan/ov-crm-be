using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Customer : BaseEntity
    {
        public int Id { get; set; }
        public string? Avatar { get; set; }
        public string AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountReferredID { get; set; }
        public string? AccountShortName { get; set; }
        public string? AccountTypeID { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? BillingCode { get; set; }
        public string? BillingCountryID { get; set; }
        public string? BillingDistrictID { get; set; }
        public string? NoOfEmployeeID { get; set; }
        public string? BillingLat { get; set; }
        public string? BillingLong { get; set; }
        public string? BillingProvinceID { get; set; }
        public string? Debt { get; set; }
        public bool IsPublic { get; set; }
        public bool IsPartner { get; set; }
        public bool IsPersonal { get; set; }
        public bool IsOldCustomer { get; set; }
        public bool IsDistributor { get; set; }
        public string? DebtLimit { get; set; }
        public string? Description { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingWardID { get; set; }
        public string? BudgetCode { get; set; }
        public string? RevenueDetail { get; set; }
        public string? BusinessTypeID { get; set; }
        public string? ContactIDAim { get; set; }
        public string? OwnerID { get; set; }
        public string? Fax { get; set; }
        public string? GenderID { get; set; }
        public string? Identification { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public string? Latitude { get; set; }
        public string? LeadSourceID { get; set; }

        public string? NumberOfDaysOwed { get; set; }
        public string? OfficeEmail { get; set; }
        public string? OfficeTel { get; set; }
        public string? OrganizationUnitID { get; set; }
        public string? SectorText { get; set; }
        public string? ShippingCode { get; set; }
        public string? ShippingCountryID { get; set; }
        public string? ShippingDistrictID { get; set; }
        public string? ShippingLat { get; set; }
        public string? ShippingLong { get; set; }
        public string? ShippingProvinceID { get; set; }
        public string? ShippingStreet { get; set; }
        public string? AnnualRevenueID { get; set; }
        public string? ShippingWardID { get; set; }

        public string? IndustryID { get; set; }

        public DateTime? CelebrateDate { get; set; }

        public DateTime? CustomerSinceDate { get; set; }

        public string? TaxCode { get; set; }
        public string? Website { get; set; }
        public int? PartnerId { get; set; }
        [NotMapped]
        public Employee Employee { get; set; }
        public Partner Partner { get; set; }
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CustomerEmployees> CustomerEmployees { get; set; } = new List<CustomerEmployees>();
        public ICollection<CustomerContacts> CustomerContacts { get; set; } = new List<CustomerContacts>();

    }
}
