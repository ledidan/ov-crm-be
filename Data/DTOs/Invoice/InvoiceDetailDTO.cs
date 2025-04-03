using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.DTOs
{
    public class InvoiceDetailDTO
    {
        public string? Id { get; set; }  // ObjectId as string

        public int? InvoiceId { get; set; }  // Reference to Invoice

        public string? InvoiceRequestName { get; set; }

        public int? ProductId { get; set; }  // Reference to Product (MySQL)

        public string? ProductCode { get; set; }

        public string? ProductName { get; set; }

        public int? TaxID { get; set; }

        public double? TaxAmount { get; set; }

        public string? TaxIDText { get; set; }

        public int? DiscountRate { get; set; }

        public double? DiscountAmount { get; set; }

        public decimal? UnitPrice { get; set; }

        public int? QuantityInstock { get; set; }

        public string? UsageUnitID { get; set; }

        public string? UsageUnitIDText { get; set; }

        public int Quantity { get; set; }

        public decimal? Total { get; set; }

        public decimal? AmountSummary { get; set; }

        public int? OrderId { get; set; }

        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }
        public int PartnerId { get; set; }

        public string? SaleOrderNo { get; set; }

        public string? PartnerName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
