using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CreateProductCatelogy
    {
        [Required]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public int PartnerId { get; set; }
    }
}
