using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.MongoModels
{
    public class Orders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("OrderCode")]
        public string OrderCode { get; set; } = null!;
        [BsonElement("TotalAmount")]
        public double TotalAmount { get; set; }

        [BsonElement("IsPaid")]
        public bool IsPaid { get; set; }
        [BsonElement("IsShared")]
        public bool? IsShared { get; set; } = false;

        [BsonElement("OrderDate")]
        public DateTime OrderDate { get; set; }

        [BsonElement("PaidDate")]
        public DateTime PaidDate { get; set; }

        [BsonElement("CustomerId")]
        public int? CustomerId { get; set; }

        // Reference to Partner ID in MySQL
        [BsonElement("PartnerId")]
        public int PartnerId { get; set; }

        // Reference to Contact ID in MySQL
        [BsonElement("ContactId")]
        public int? ContactId { get; set; }

        // References to Employee IDs in MySQL
        [BsonElement("EmployeeAccessLevels")]
        public List<EmployeeAccess> EmployeeAccessLevels { get; set; } = new();

        [BsonElement("OrderDetails")]
        public List<OrderDetails> OrderDetails { get; set; }
    }
}
