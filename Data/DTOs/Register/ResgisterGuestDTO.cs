

using System.ComponentModel.DataAnnotations;

namespace Data.DTOs {
    public class RegisterGuestDTO
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
        
        [DataType(DataType.PhoneNumber)]
        [Phone]
        [Required]
        public required string Phone { get; set; }

        // [DataType(DataType.Password)]
        // [Required]
        // public required string Password { get; set; }

        [DataType(DataType.Text)]
        public required string FullName { get; set; }

    }
}