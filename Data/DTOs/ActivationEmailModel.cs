


namespace Data.DTOs
{
    public class ActivationEmailModel
    {
        public string FullName { get; set; }
        public string VerificationLink { get; set; }
        public string Email { get; set; }
        public string ActivationCode { get; set; }

        public string AppName { get; set; }

    }
}