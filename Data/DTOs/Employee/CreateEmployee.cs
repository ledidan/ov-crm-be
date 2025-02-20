using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CreateEmployee
    {
        [Required]
        public required string Fullname { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        [Required]
        public required string StreetAddress { get; set; }
        [Required]
        public required string District { get; set; }
        [Required]
        public required string Province { get; set; }
        [Required]
        public required string Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public DateTime SignedContractDate { get; set; }
        [Required]
        public int PartnerId { get; set; }
    }
}
