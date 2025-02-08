using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class Register
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string FullName { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Required]
        public required string ConfirmPassword { get; set; }
        [Required]
        public int PartnerId { get; set; }
        public int EmployeeId { get; set; }
    }
}
