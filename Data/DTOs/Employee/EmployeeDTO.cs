
using Data.Enums;

namespace Data.DTOs
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public required string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? OfficePhone { get; set; }
        public int CRMRoleId { get; set; }
        public string? OfficeEmail { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public int? JobPositionGroupId { get; set; }
        public int? JobTitleGroupId { get; set; }
        public JobStatus JobStatus { get; set; }
        public DateTime? SignedContractDate { get; set; }
        public DateTime? SignedProbationaryContract { get; set; }
        public DateTime? Resignation { get; set; }
        public int PartnerId { get; set; }
        public List<ContactDTO> Contacts { get; set; }

        public List<int> ContactIds { get; set; } = new List<int>();
    }
}