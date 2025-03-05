using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class SetPasswordDTO
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public required string Email { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Required]
        public required string ConfirmPassword { get; set; }
    }
}
