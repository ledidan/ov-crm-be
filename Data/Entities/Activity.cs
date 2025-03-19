
namespace Data.Entities
{
    public class Activity : BaseEntity
    {
        public int Id { get; set; }
        public string? TagID { get; set; }
        public string? TagColor { get; set; }
        public bool? IsDeleted { get; set; }
        public string? ActivityName { get; set; }
        public string? ActivityCategory { get; set; }
        public DateTime? DueDate { get; set; }
        public string? StatusID { get; set; }
        public string? PriorityID { get; set; }
        public bool? IsSendNotificationEmail { get; set; }
        public bool? IsRepeat { get; set; }
        public bool? IsReminder { get; set; }
        public string? Description { get; set; }
        public string? ModuleType { get; set; }
        public string? RemindID { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public string? Place { get; set; }
        public bool? Duplicate { get; set; }
        public bool? SendEmail { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsOpen { get; set; }
        public bool? IsAllDay { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OfficeEmail { get; set; }
        public int? CustomerId { get; set; }
        public int PartnerId { get; set; }
        public int? TaskOwnerId { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ContactId { get; set; }
        public int? OrderId { get; set; }
        public int? InvoiceId { get; set; }
        public int? RelatedUsersID { get; set; }
        public Appointment? Appointment { get; set; }
        public Mission? Mission { get; set; }
        public Call? Call { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();
    }
}