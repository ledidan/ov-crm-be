using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CreateCustomer
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        [Required]
        public required string StreetAddress { get; set; }
        [Required]
        public required string District { get; set; }
        [Required]
        public required string Province { get; set; }
        public string? Email { get; set; }
        [Required]
        public int PartnerId { get; set; }
    }
}
