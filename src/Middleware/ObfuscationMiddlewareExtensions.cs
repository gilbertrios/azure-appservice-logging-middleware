namespace AzureAppServiceLoggingMiddleware.Middleware;

/// <summary>
/// Extension methods for registering ObfuscationMiddleware.
/// </summary>
public static class ObfuscationMiddlewareExtensions
{
    /// <summary>
    /// Adds ObfuscationMiddleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseObfuscationMiddleware(
        this IApplicationBuilder app,
        Action<ObfuscationOptions>? configureOptions = null)
    {
        var options = new ObfuscationOptions();
        configureOptions?.Invoke(options);

        if (!options.Enabled)
            return app;

        return app.UseMiddleware<ObfuscationMiddleware>(options);
    }

    /// <summary>
    /// Adds ObfuscationMiddleware configuration to services.
    /// </summary>
    public static IServiceCollection AddObfuscationMiddleware(
        this IServiceCollection services,
        Action<ObfuscationOptions> configureOptions)
    {
        var options = new ObfuscationOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);
        return services;
    }
}
