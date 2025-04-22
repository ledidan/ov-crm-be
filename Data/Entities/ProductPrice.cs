using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ProductPrice : BaseEntity
    {
        public int Id { get; set; }
        public virtual required Product Product { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public bool IsLatest { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
