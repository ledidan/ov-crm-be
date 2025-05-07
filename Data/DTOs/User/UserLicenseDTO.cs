

namespace Data.DTOs
{
    public class UserLicenseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int? EmployeeId { get; set; }
        public List<LicenseForUserDTO> Licenses { get; set; } = new();
    }
}