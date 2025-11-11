namespace AzureAppServiceLoggingMiddleware.Infrastructure
{
    /// <summary>
    /// Defines a self-contained module that can register services and map endpoints.
    /// Useful for organizing Minimal APIs by domain (Orders, Users, Payments, etc.)
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Register module-specific services and dependencies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        IServiceCollection RegisterModule(IServiceCollection services);

        /// <summary>
        /// Map module-specific endpoints
        /// </summary>
        /// <param name="endpoints">The endpoint route builder</param>
        /// <returns>The endpoint route builder for chaining</returns>
        IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
    }
}