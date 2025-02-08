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

        public IMongoCollection<Orders> Orders =>
            _mongoDatabase.GetCollection<Orders>("Orders");
        public IMongoCollection<OrderDetails> OrderDetails =>
            _mongoDatabase.GetCollection<OrderDetails>("OrderDetails");

    }
}
