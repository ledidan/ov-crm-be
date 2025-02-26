
namespace Data.DTOs
{
    public class CallDTO
    {
        public int Id { get; set; }
        public string? TagID { get; set; }
        public string? TagColor { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DueDate { get; set; }
        public string? StatusID { get; set; }
        public string? PriorityID { get; set; }
        public DateTime? CallStart { get; set; }
        public int? CallDuration { get; set; }
        public string? Description { get; set; }
        public string? ModuleType { get; set; }

        public string? ActivityName { get; set; }
        public string? CallName { get; set; }
        public string? CallGoalID { get; set; }
        public string? CallTypeID { get; set; }
        public bool? CallDone { get; set; }
        public string? CallResult { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime? EventEnd { get; set; }
        public bool? Duplicate { get; set; }
        public bool? SendEmail { get; set; }
        public string? CallID { get; set; }
        public string? CallRecord { get; set; }
        public DateTime CallEnd { get; set; }
        public string? CallResultID { get; set; }
        public string? PhoneNumber { get; set; }
        public int? CustomerId { get; set; }
        public int PartnerId { get; set; }
        public int? TaskOwnerId { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ContactId { get; set; }
        public int? RelatedUsersID { get; set; }
        // public List<Employee> Employees { get; set; } = new List<Employee>();
        // public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();

    }
}