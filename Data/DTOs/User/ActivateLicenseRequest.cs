

namespace Data.DTOs
{
    public class ActivateLicenseRequest
    {
        public string Code { get; set; }
        public string Email { get; set; }

        public int ApplicationId { get; set; }
    }
}