using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CreateProduct
    {
        [Required]
        public required string Code { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Unit { get; set; }
        public string? ProducerName { get; set; }
        public int WarrantyPeriodPerMonth { get; set; }
        [Required]
        public int ProductCatelogyId { get; set; }
        [Required]
        public double SellingPrice { get; set; }
    }
}
