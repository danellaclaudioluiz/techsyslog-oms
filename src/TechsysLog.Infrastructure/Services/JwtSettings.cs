namespace TechsysLog.Infrastructure.Services;

/// <summary>
/// JWT configuration settings.
/// Mapped from appsettings.json.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpirationInMinutes { get; set; } = 60;
}