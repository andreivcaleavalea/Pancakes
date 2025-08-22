namespace BlogService.Configuration;

/// <summary>
/// Strongly typed JWT configuration used by services that need to read claims.
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PancakesBlog";
    public string Audience { get; set; } = "PancakesBlogUsers";
}
