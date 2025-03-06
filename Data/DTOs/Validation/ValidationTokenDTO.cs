


namespace Data.DTOs
{
    public class ValidateTokenDto
    {
        public string? Email { get; set; } = null;

        public string? PhoneNumber { get; set; } = null;
        public string Token { get; set; }
    }
}