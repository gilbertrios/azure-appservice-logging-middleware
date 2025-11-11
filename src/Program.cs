using AzureAppServiceLoggingMiddleware.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Map all module endpoints
app.MapEndpoints();

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
