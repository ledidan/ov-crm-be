

namespace Data.DTOs
{
    public class ProductDTO
    {   
        public int? Id { get; set;}
        public string? ProductCode { get; set; }
        public string? ProductGroupID { get; set; }
        public string? ProductGroupName { get; set; }
        public string? ProductName { get; set; }
        public decimal? AmountSummary { get; set; }
        public string? Avatar { get; set; }
        public decimal? ConversionRate { get; set; }
        public string? ConversionUnit { get; set; }
        public string? CreatedBy { get; set; }
        public string? Description { get; set; }
        public string? Equation { get; set; }
        public bool? Inactive { get; set; }
        public string? InventoryItemID { get; set; }
        public bool? IsFollowSerialNumber { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsSetProduct { get; set; }
        public bool? IsSystem { get; set; }
        public bool? IsUseTax { get; set; }
        public string? ModifiedBy { get; set; }
        public string? OldProductCode { get; set; }
        public string? OperatorID { get; set; }
        public bool? PriceAfterTax { get; set; }
        public string? ProductPropertiesID { get; set; }
        public decimal? PurchasedPrice { get; set; }
        public int? QuantityDemanded { get; set; }
        public string? QuantityFormula { get; set; }
        public int? QuantityInstock { get; set; }
        public int? QuantityOrdered { get; set; }
        public string? SaleDescription { get; set; }
        public string? SearchTagID { get; set; }
        public string? TagColor { get; set; }
        public string? TaxID { get; set; }
        public bool? Taxable { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? UnitPrice1 { get; set; }
        public decimal? UnitPrice2 { get; set; }
        public decimal? UnitPriceFixed { get; set; }
        public string? UsageUnitID { get; set; }
        public string? VendorNameID { get; set; }
        public string? WarrantyDescription { get; set; }
        public string? WarrantyPeriod { get; set; }
        public string? WarrantyPeriodTypeID { get; set; }
        public int? OwnerID { get; set; }
        public int? ProductCategoryId { get; set; }

    }
}