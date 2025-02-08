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
        public string OrderId { get; set; } = null!; // Reference to Order 

        [BsonElement("ProductId")]
        public int? ProductId { get; set; } // Reference to Product (MySQL)

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("SellingPrice")]
        public double SellingPrice { get; set; }

        [BsonElement("Amount")]
        public double Amount { get; set; }
    }
}
