using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Infrastructure.Persistence;

/// <summary>
/// MongoDB class mappings for domain entities and value objects.
/// </summary>
public static class MongoDbMappings
{
    private static bool _registered;

    public static void RegisterMappings()
    {
        if (_registered) return;

        // Configure conventions
        var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register("TechsysLogConventions", conventionPack, _ => true);

        // BaseEntity - map Id as document _id
        BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(e => e.Id)
                .SetIdGenerator(GuidGenerator.Instance)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            cm.SetIsRootClass(true);
        });

        // AggregateRoot - ignore DomainEvents
        BsonClassMap.RegisterClassMap<AggregateRoot>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.UnmapMember(a => a.DomainEvents);
        });

        // Value Objects - need MapCreator for private constructors
        BsonClassMap.RegisterClassMap<Email>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(e => e.Value).SetElementName("value");
            cm.MapCreator(e => Email.Create(e.Value).Value);
        });

        BsonClassMap.RegisterClassMap<Password>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(p => p.Hash).SetElementName("hash");
            cm.MapCreator(p => Password.FromHash(p.Hash));
        });

        BsonClassMap.RegisterClassMap<OrderNumber>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(o => o.Value).SetElementName("value");
            cm.MapCreator(o => OrderNumber.Create(o.Value).Value);
        });

        BsonClassMap.RegisterClassMap<Cep>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.Value).SetElementName("value");
            cm.MapCreator(c => Cep.Create(c.Value).Value);
        });

        BsonClassMap.RegisterClassMap<Address>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(a => a.Cep).SetElementName("cep");
            cm.MapMember(a => a.Street).SetElementName("street");
            cm.MapMember(a => a.Number).SetElementName("number");
            cm.MapMember(a => a.Neighborhood).SetElementName("neighborhood");
            cm.MapMember(a => a.City).SetElementName("city");
            cm.MapMember(a => a.State).SetElementName("state");
            cm.MapMember(a => a.Complement).SetElementName("complement");
            cm.MapCreator(a => Address.Create(
                a.Cep,
                a.Street,
                a.Number,
                a.Neighborhood,
                a.City,
                a.State,
                a.Complement).Value);
        });

        // Entities
        BsonClassMap.RegisterClassMap<User>(cm =>
        {
            cm.AutoMap();
        });

        BsonClassMap.RegisterClassMap<Order>(cm =>
        {
            cm.AutoMap();
        });

        BsonClassMap.RegisterClassMap<Delivery>(cm =>
        {
            cm.AutoMap();
        });

        BsonClassMap.RegisterClassMap<Notification>(cm =>
        {
            cm.AutoMap();
        });

        _registered = true;
    }
}