using Testcontainers.MongoDb;
using TechsysLog.Infrastructure.Persistence;

namespace TechsysLog.Integration.Tests.Fixtures;

/// <summary>
/// Shared MongoDB container fixture for integration tests.
/// </summary>
public class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container;
    public MongoDbContext Context { get; private set; } = null!;
    public MongoDbSettings Settings { get; private set; } = null!;

    public MongoDbFixture()
    {
        _container = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Settings = new MongoDbSettings
        {
            ConnectionString = _container.GetConnectionString(),
            DatabaseName = $"techsyslog_test_{Guid.NewGuid():N}"
        };

        Context = new MongoDbContext(Settings);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}