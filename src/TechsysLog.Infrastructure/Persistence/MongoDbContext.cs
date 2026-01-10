using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Infrastructure.Persistence;

/// <summary>
/// MongoDB database context.
/// Provides access to collections.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        // Configure Guid serialization globally
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        
        // Register class mappings
        MongoDbMappings.RegisterMappings();
    }

    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);

        ConfigureCollections();
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    public IMongoCollection<Delivery> Deliveries => _database.GetCollection<Delivery>("deliveries");
    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");

    private void ConfigureCollections()
    {
        // Users indexes
        var usersIndexes = Users.Indexes;
        usersIndexes.CreateOne(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending("Email.Value"),
            new CreateIndexOptions { Unique = true }));

        // Orders indexes
        var ordersIndexes = Orders.Indexes;
        ordersIndexes.CreateOne(new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending("OrderNumber.Value"),
            new CreateIndexOptions { Unique = true }));
        ordersIndexes.CreateOne(new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(o => o.UserId)));
        ordersIndexes.CreateOne(new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(o => o.Status)));
        ordersIndexes.CreateOne(new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(o => o.CreatedAt)));

        // Deliveries indexes
        var deliveriesIndexes = Deliveries.Indexes;
        deliveriesIndexes.CreateOne(new CreateIndexModel<Delivery>(
            Builders<Delivery>.IndexKeys.Ascending(d => d.OrderId),
            new CreateIndexOptions { Unique = true }));
        deliveriesIndexes.CreateOne(new CreateIndexModel<Delivery>(
            Builders<Delivery>.IndexKeys.Ascending(d => d.DeliveredBy)));

        // Notifications indexes
        var notificationsIndexes = Notifications.Indexes;
        notificationsIndexes.CreateOne(new CreateIndexModel<Notification>(
            Builders<Notification>.IndexKeys.Ascending(n => n.UserId)));
        notificationsIndexes.CreateOne(new CreateIndexModel<Notification>(
            Builders<Notification>.IndexKeys.Combine(
                Builders<Notification>.IndexKeys.Ascending(n => n.UserId),
                Builders<Notification>.IndexKeys.Ascending(n => n.Read))));
    }
}