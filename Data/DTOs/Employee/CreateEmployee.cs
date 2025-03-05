using System.ComponentModel.DataAnnotations;
using Data.Entities;
using Data.Enums;

namespace Data.DTOs
{
    public class CreateEmployee
    {

        [Required]
        public string EmployeeCode { get; set; }
        [Required]
        public required string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? StreetAddress { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? OfficePhone { get; set; }
        public string? OfficeEmail { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public int? JobPositionGroupId { get; set; }
        public int? JobTitleGroupId { get; set; }
        public JobStatus JobStatus { get; set; }
        public DateTime SignedContractDate { get; set; }
        public DateTime? SignedProbationaryContract { get; set; }
        public DateTime? Resignation { get; set; }
        [Required]
        public int PartnerId { get; set; }
    }
}
