

namespace Data.DTOs
{
    public class UpdateProductCategoryDTO
    {
        public string? Avatar { get; set; }
        public string? ProductCategoryCode { get; set; }
        public string? ProductCategoryName { get; set; }
        public string? InventoryCategoryID { get; set; }
        public int? ParentProductCategoryID { get; set; } = 0;
        public bool? IsPublic { get; set; }
        public bool? InActive { get; set; }
        public string? Description { get; set; }
        public string? ModifiedBy { get; set; }
    }
}