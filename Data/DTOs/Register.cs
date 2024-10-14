using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class Register
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Fullname { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Required]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string? Role { get; set; }
    }
}
