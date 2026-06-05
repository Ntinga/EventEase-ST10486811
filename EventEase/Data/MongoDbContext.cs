using Microsoft.Extensions.Configuration;
using EventEase.Models;
using MongoDB.Driver;

namespace EventEase.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }
        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
        public IMongoCollection<Event> Events => _database.GetCollection<Event>("Events");
        public IMongoCollection<EventType> EventTypes => _database.GetCollection<EventType>("EventTypes");
        public IMongoCollection<Venue> Venues => _database.GetCollection<Venue>("Venues");
    }
}
