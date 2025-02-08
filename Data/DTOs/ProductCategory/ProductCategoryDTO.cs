

namespace Data.DTOs
{
    public class ProductCategoryDTO
    {
        public int Id { get; set; }
        public string? Avatar { get; set; }
        public string? ProductCategoryCode { get; set; }
        public string? ProductCategoryName { get; set; }
        public List<ProductCategoryDTO> SubCategories { get; set; } = new List<ProductCategoryDTO>();
        public string? InventoryCategoryID { get; set; }
        public int? ParentProductCategoryID { get; set; }
        public bool? IsPublic { get; set; }
        public bool? InActive { get; set; }
        public string? Description { get; set; }
        public string? ModifiedBy { get; set; }
        public int? OwnerId { get; set; }
        // public int? PartnerId { get; set; }
    }
}