using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using EventEase.Models;

namespace EventEase.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            var client = new MongoClient(config["MongoSettings:ConnectionString"]);
            _database = client.GetDatabase(config["MongoSettings:DatabaseName"]);
        }

        public IMongoCollection<Event> Events => _database.GetCollection<Event>("Events");
        public IMongoCollection<EventType> EventTypes => _database.GetCollection<EventType>("EventTypes");
    }
}