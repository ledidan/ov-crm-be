using Microsoft.EntityFrameworkCore;

namespace Data.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public string? ProducerName { get; set; }
        public int WarrantyPeriodPerMonth { get; set; }
        public virtual required ProductCatelogy ProductCatelogy { get; set; }
        public virtual required Partner Partner { get; set; }
    }
}
