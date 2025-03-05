using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class RegisterUserDTO
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string FullName { get; set; }

        public string? Phone { get; set; }

        public string? Avatar { get; set; }

        public DateTime? Birthday { get; set; }
        [Required]
        public int PartnerId { get; set; }
        public int EmployeeId { get; set; }
    }
}
