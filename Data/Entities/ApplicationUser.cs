

namespace Data.Entities
{
    public class ApplicationUser : BaseEntity
    {
        public int Id { get; set; }
        public string? FullName { get; set; }

        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; } = false;

        public bool? IsActivateEmail { get; set; } = false;

        public int? EmployeeId { get; set; }
    }
}
