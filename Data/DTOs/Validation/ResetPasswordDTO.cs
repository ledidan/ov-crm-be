using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class ResetPasswordDTO
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public required string Email { get; set; }

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
