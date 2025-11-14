using AzureAppServiceLoggingMiddleware.Infrastructure;
using AzureAppServiceLoggingMiddleware.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Azure App Service API",
        Version = "v1",
        Description = "Minimal API with Orders and Payments modules"
    });
});

// Register all modules (auto-discovers OrderModule and PaymentModule)
builder.Services.RegisterModules();

// Configure obfuscation middleware
builder.Services.AddObfuscationMiddleware(options =>
{
    options.Enabled = builder.Configuration.GetValue<bool>("ObfuscationMiddleware:Enabled", true);
    options.ObfuscationMask = builder.Configuration.GetValue<string>("ObfuscationMiddleware:ObfuscationMask") 
        ?? "***REDACTED***";
    
    var sensitiveProps = builder.Configuration.GetSection("ObfuscationMiddleware:SensitiveProperties").Get<List<string>>();
    if (sensitiveProps != null)
    {
        options.SensitiveProperties = sensitiveProps;
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Enable obfuscation middleware (must be before UseHttpsRedirection to capture full bodies)
app.UseObfuscationMiddleware();

app.UseHttpsRedirection();

// Map all module endpoints
app.MapEndpoints();

// Root endpoint for Azure health probes
app.MapGet("/", () => Results.Ok(new 
{ 
    status = "Healthy",
    service = "Azure Logging Middleware API",
    timestamp = DateTime.UtcNow
}))
.WithName("Root")
.ExcludeFromDescription();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}))
.WithName("HealthCheck")
.WithTags("Health")
.WithSummary("Check API health status");

app.Run();

// Make the Program class accessible to integration tests
public partial class Program { }
