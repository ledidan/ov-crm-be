

namespace Data.DTOs
{
    public class ResetPasswordModel
    {
        public string VerificationLink { get; set; }

        public string Email { get; set; }
        
        public string Token { get; set; }

        public string PhoneNumber { get; set; }
        
    }
}

