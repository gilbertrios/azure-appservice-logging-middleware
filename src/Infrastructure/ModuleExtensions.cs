namespace AzureAppServiceLoggingMiddleware.Infrastructure
{
    /// <summary>
    /// Extension methods for registering and mapping modules in a Minimal API application
    /// </summary>
    public static class ModuleExtensions
    {
        // Store discovered modules to avoid re-discovery
        private static readonly List<IModule> registeredModules = new();

        /// <summary>
        /// Discovers and registers all modules found in the assembly.
        /// Automatically calls RegisterModule on each discovered module.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection RegisterModules(this IServiceCollection services)
        {
            var modules = DiscoverModules();
            foreach (var module in modules)
            {
                // Log module registration for visibility
                Console.WriteLine($"Registering module: {module.GetType().Name}");
                module.RegisterModule(services);
                registeredModules.Add(module);
            }

            return services;
        }

        /// <summary>
        /// Maps endpoints for all registered modules.
        /// Call this after app.Build() in your Program.cs
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The web application for chaining</returns>
        public static WebApplication MapEndpoints(this WebApplication app)
        {
            foreach (var module in registeredModules)
            {
                Console.WriteLine($"Mapping endpoints for module: {module.GetType().Name}");
                module.MapEndpoints(app);
            }
            return app;
        }

        /// <summary>
        /// Discovers all types implementing IModule in the current assembly
        /// </summary>
        /// <returns>Instances of all discovered modules</returns>
        private static IEnumerable<IModule> DiscoverModules()
        {
            return typeof(IModule).Assembly
                .GetTypes()
                .Where(p => p.IsClass && !p.IsAbstract && p.IsAssignableTo(typeof(IModule)))
                .Select(Activator.CreateInstance)
                .Cast<IModule>();
        }
    }
}