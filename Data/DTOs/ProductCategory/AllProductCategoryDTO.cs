
namespace Data.DTOs
{
    public class AllProductCategoryDTO
    {
        public int Id { get; set; }
        public string? Avatar { get; set; }
        public string? ProductCategoryCode { get; set; }
        public string? ProductCategoryName { get; set; }
        public int? ParentProductCategoryID { get; set; }

        public bool? InActive { get; set; }

        public string? Description { get; set; }
    }
}