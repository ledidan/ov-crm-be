using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class Login
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public required string Email { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public required string Password { get; set; }
    }
}
