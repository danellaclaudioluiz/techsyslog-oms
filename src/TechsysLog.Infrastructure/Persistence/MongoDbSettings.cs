namespace TechsysLog.Infrastructure.Persistence;

/// <summary>
/// MongoDB connection settings.
/// Mapped from appsettings.json.
/// </summary>
public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}