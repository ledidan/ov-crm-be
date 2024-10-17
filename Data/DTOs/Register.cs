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
        public required string Fullname { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Required]
        public required string ConfirmPassword { get; set; }

        public int PartnerId { get; set; }
    }
}
