namespace AzureAppServiceLoggingMiddleware.Middleware;

/// <summary>
/// Configuration options for the ObfuscationMiddleware.
/// </summary>
public class ObfuscationOptions
{
    /// <summary>
    /// List of property names that should be obfuscated in request/response bodies.
    /// Property name matching is case-insensitive.
    /// </summary>
    public List<string> SensitiveProperties { get; set; } = new();

    /// <summary>
    /// The mask string to replace sensitive values with.
    /// Default: "***REDACTED***"
    /// </summary>
    public string ObfuscationMask { get; set; } = "***REDACTED***";

    /// <summary>
    /// Whether to enable obfuscation middleware.
    /// Default: true
    /// </summary>
    public bool Enabled { get; set; } = true;
}
