
using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class UpdateMissionDTO
    {
        public string? TagID { get; set; }
        public string? TagColor { get; set; }
        public bool? IsDeleted { get; set; }
        [Required]
        public string? ActivityName { get; set; }
        public string? MissionName { get; set; }
        public string? MissionTypeID { get; set; }
        [Required]
        public DateTime? DueDate { get; set; }
        public string? StatusID { get; set; }
        public string? PriorityID { get; set; }
        public bool? IsRepeat { get; set; }
        public bool? IsReminder { get; set; }
        public string? Description { get; set; }
        public bool? Duplicate { get; set; }
        public bool? SendEmail { get; set; }
        public bool? IsPublic { get; set; }
        public int? CustomerId { get; set; }
        public int? TaskOwnerId { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ContactId { get; set; }
        public int? RelatedUsersID { get; set; }
        // public List<Employee> Employees { get; set; } = new List<Employee>();
        // public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();

    }
}