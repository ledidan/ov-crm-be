
using Data.DTOs.Contact;

namespace Data.DTOs
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public DateTime SignedContractDate { get; set; }
        public int PartnerId { get; set; }
        public List<ContactDTO> Contacts { get; set; }

        public List<int> ContactIds { get; set; } = new List<int>();
    }
}