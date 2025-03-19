




using Data.Enums;

namespace Data.DTOs
{
    public class MergedEmployeeUserDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string? EmployeeFullName { get; set; }
        public string? EmployeeGender { get; set; }
        public DateTime? EmployeeDOB { get; set; }
        public string? EmployeePhone { get; set; }
        public string? EmployeeEmail { get; set; }
        public string? EmployeeAddress { get; set; }
        public string? OfficePhone { get; set; }
        public string? OfficeEmail { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public JobStatus JobStatus { get; set; }
        public DateTime? SignedProbationaryContract { get; set; }
        public DateTime? Resignation { get; set; }
        public DateTime? SignedContractDate { get; set; }
        public int PartnerId { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? Avatar { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? UserGender { get; set; }
        public DateTime? UserDOB { get; set; }
        public bool? IsActive { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public bool? IsActivateEmail { get; set; }
    }

}