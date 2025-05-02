using System.Text.Json.Serialization;

namespace Data.DTOs
{
    public class OrderDetailDTO
    {
        public string? Id { get; set; }

        public int? OrderId { get; set; }

        public string? SaleOrderNo { get; set; }

        public int PartnerId { get; set; }
        public string? PartnerName { get; set; }
        public int ProductId { get; set; }
        public string? Avatar { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }

        public int? TaxID { get; set; }
        public double? TaxAmount { get; set; }
        public string? TaxIDText { get; set; }

        public int? DiscountRate { get; set; }
        public double? DiscountAmount { get; set; }

        public decimal? UnitPrice { get; set; }
        public int? QuantityInstock { get; set; }
        public string? InventoryItemID { get; set; }
        public string? UsageUnitID { get; set; }
        public string? UsageUnitIDText { get; set; }

        public int Quantity { get; set; }

        public decimal? Total { get; set; }

        public decimal? AmountSummary { get; set; }

        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
