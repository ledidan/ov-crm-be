using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.MongoModels
{
    public class OrderDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("OrderId")]
        public int? OrderId { get; set; } = null!; // Reference to Order 

        [BsonElement("SaleOrderNo")]
        public string? SaleOrderNo { get; set; }
        [BsonElement("ProductId")]
        public int? ProductId { get; set; } // Reference to Product (MySQL)

        [BsonElement("ProductCode")]
        public string? ProductCode { get; set; }

        [BsonElement("Avatar")]
        public string? Avatar { get; set; }

        
        [BsonElement("ProductName")]
        public string? ProductName { get; set; }

        [BsonElement("TaxID")]
        public int? TaxID { get; set; }

        [BsonElement("TaxAmount")]
        public double? TaxAmount { get; set; }

        [BsonElement("TaxIDText")]
        public string? TaxIDText { get; set; }

        [BsonElement("DiscountRate")]
        public int? DiscountRate { get; set; }

        [BsonElement("DiscountAmount")]
        public double? DiscountAmount { get; set; }

        [BsonElement("UnitPrice")]
        public decimal? UnitPrice { get; set; }

        [BsonElement("QuantityInstock")]
        public int? QuantityInstock { get; set; }

        [BsonElement("UsageUnitID")]
        public string? UsageUnitID { get; set; }

        [BsonElement("UsageUnitIDText")]
        public string? UsageUnitIDText { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("InventoryItemID")]
        public string? InventoryItemID { get; set; }

        [BsonElement("Total")]
        public decimal? Total { get; set; }

        [BsonElement("AmountSummary")]
        public decimal? AmountSummary { get; set; }

        [BsonElement("PartnerId")]
        public int PartnerId { get; set; }

        [BsonElement("PartnerName")]
        public string? PartnerName { get; set; }

        [BsonElement("CustomerId")]
        public int? CustomerId { get; set; }

        [BsonElement("CustomerName")]
        public string? CustomerName { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
