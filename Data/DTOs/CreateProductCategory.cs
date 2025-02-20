using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.DTOs
{
    public class CreateProductCategory
    {
        [Required]
        public required string ProductCategoryCode { get; set; }
        [Required]
        public string? ProductCategoryName { get; set; }
        public string? InventoryCategoryID { get; set; }
        public string? Avatar { get; set; }
        public int? ParentProductCategoryID { get; set; }
        public bool? IsPublic { get; set; }
        public bool? InActive { get; set; }
        public string? Description { get; set; }

        public string? ModifiedBy { get; set; }
        // public int? OwnerId { get; set; }
        // public int? PartnerId { get; set; }
    }
}
