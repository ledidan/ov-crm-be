using Data.MongoModels;
using MongoDB.Driver;

namespace ServerLibrary.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbContext(IMongoClient mongoClient, string databaseName)
        {
            _mongoDatabase = mongoClient.GetDatabase(databaseName);
        }
        public IMongoCollection<OrderDetails> OrderDetails =>
        _mongoDatabase.GetCollection<OrderDetails>("OrderDetails");
        public IMongoCollection<InvoiceDetails> InvoiceDetails =>
        _mongoDatabase.GetCollection<InvoiceDetails>("InvoiceDetails");
        public IMongoCollection<QuoteDetails> QuoteDetails =>
        _mongoDatabase.GetCollection<QuoteDetails>("QuoteDetails");
        public IMongoCollection<OpportunityProductDetails> OpportunityProductDetails =>
        _mongoDatabase.GetCollection<OpportunityProductDetails>("OpportunityProductDetails");

    }
}
