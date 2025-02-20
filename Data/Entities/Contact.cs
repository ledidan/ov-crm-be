
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Contact : BaseEntity
    {
        public int Id { get; set; }
        public string? AccountTypeID { get; set; }
        public string? ContactCode { get; set; }
        public string? ContactName { get; set; }
        public string? DepartmentID { get; set; }
        public string? LeadSourceID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? OfficeTel { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Description { get; set; }
        public string? MailingDistrictID { get; set; }
        public string? MailingProvinceID { get; set; }
        public string? MailingStreet { get; set; }
        public string? MailingWardID { get; set; }
        public string? MailingZip { get; set; }
        public string? Mobile { get; set; }
        public string? OfficeEmail { get; set; }
        public string? OtherPhone { get; set; }
        public string? SalutationID { get; set; }
        public string? ShippingDistrictID { get; set; }
        public string? ShippingProvinceID { get; set; }
        public string? ShippingStreet { get; set; }
        public string? ShippingWardID { get; set; }
        public string? ShippingZip { get; set; }
        public string? TitleID { get; set; }
        public string? Zalo { get; set; }
        public bool? IsPublic { get; set; } = false;
        public bool? EmailOptOut { get; set; } = false;
        public bool? PhoneOptOut { get; set; } = false;
        public int PartnerId { get; set; }
        public int? EmployeeId { get; set; }
        public virtual required Partner Partner { get; set; }
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Employee> Employees { get; set; } = new List<Employee>();

        public List<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ContactEmployees> ContactEmployees { get; set; } = new List<ContactEmployees>();
    }
}