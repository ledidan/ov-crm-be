using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class PasswordResetTokens : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string Token { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
