using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class ResetPasswordDTO
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; } = string.Empty;

        [Required]
        public required string Token { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public required string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        [Required]
        public required string ConfirmNewPassword { get; set; }
    }
}
