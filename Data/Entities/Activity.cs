
namespace Data.Entities
{
    public class Activity : BaseEntity
    {
        public int Id { get; set; }
        public string? ActivityName { get; set; }
        public int? ModuleType { get; set; }
        public string? ModuleTypeText { get; set; }
        public string? DueDate { get; set; }
        public string? EndTime { get; set; }
        public string? Place { get; set; }
        public string? StartTimeCustom { get; set; }

        public string? Description { get; set; }

        public List<Employee> Employees { get; set; } = new List<Employee>();

        public virtual required Customer Customer { get; set; }

        public virtual Status? Status { get; set; }
        public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();

    }
}