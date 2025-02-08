using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data.DTOs
{
    public class Login
    {
        [DataType(DataType.EmailAddress)]
        [JsonPropertyName("Email")]

        [EmailAddress]
        [Required]
        public required string Email { get; set; }

        [DataType(DataType.Password)]
        [JsonPropertyName("Password")]
        [Required]
        public required string Password { get; set; }
    }
}
