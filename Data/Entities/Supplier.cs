using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class Supplier : BaseEntity
    {
        [Key]
        public int Id { get; set; } // Khóa chính

        public required string SupplierName { get; set; } // Tên nhà cung cấp

        public string? ContactInfo { get; set; } // Thông tin liên hệ (tùy chọn)

        public string? Address { get; set; } // Địa chỉ (tùy chọn)

        public virtual ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
    }
}