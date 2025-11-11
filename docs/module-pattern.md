# Module Organization Pattern for Minimal APIs

## Overview

The `IModule` interface provides a clean way to organize Minimal APIs by domain/feature rather than having all endpoints in `Program.cs`.

## Key Improvements Made

### 1. **Added XML Documentation**
- Clear documentation for interface and methods
- Helps with IntelliSense and maintainability

### 2. **Added `!p.IsAbstract` Check**
- Prevents attempting to instantiate abstract classes
- Avoids runtime errors during module discovery

### 3. **Added Logging**
- Console output shows which modules are registered/mapped
- Helpful for debugging and visibility

### 4. **Better Naming**
- Changed `builder` parameter to `services` for clarity
- More consistent with ASP.NET Core conventions

## Additional Suggestions

### Option 1: Add Module Metadata (Recommended)

```csharp
/// <summary>
/// Extended module interface with metadata for better organization
/// </summary>
public interface IModule
{
    /// <summary>
    /// Module name (e.g., "Orders", "Users", "Payments")
    /// </summary>
    string Name => GetType().Name.Replace("Module", "");

    /// <summary>
    /// Module route prefix (e.g., "/api/orders")
    /// </summary>
    string RoutePrefix { get; }

    /// <summary>
    /// Module version for API versioning
    /// </summary>
    string? Version => null;

    IServiceCollection RegisterModule(IServiceCollection services);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
```

### Option 2: Add Module Ordering

```csharp
public interface IModule
{
    /// <summary>
    /// Controls the order in which modules are registered (lower first)
    /// </summary>
    int Order => 100;

    IServiceCollection RegisterModule(IServiceCollection services);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}

// Then in DiscoverModules():
private static IEnumerable<IModule> DiscoverModules()
{
    return typeof(IModule).Assembly
        .GetTypes()
        .Where(p => p.IsClass && !p.IsAbstract && p.IsAssignableTo(typeof(IModule)))
        .Select(Activator.CreateInstance)
        .Cast<IModule>()
        .OrderBy(m => m.Order); // Sort by order
}
```

### Option 3: Add Tags for OpenAPI/Swagger Grouping

```csharp
public interface IModule
{
    /// <summary>
    /// Tags for Swagger/OpenAPI grouping
    /// </summary>
    string[] Tags => new[] { Name };

    IServiceCollection RegisterModule(IServiceCollection services);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
```

### Option 4: Use ILogger Instead of Console.WriteLine

```csharp
public static IServiceCollection RegisterModules(
    this IServiceCollection services, 
    ILogger? logger = null)
{
    var modules = DiscoverModules();
    foreach (var module in modules)
    {
        logger?.LogInformation("Registering module: {ModuleName}", module.GetType().Name);
        module.RegisterModule(services);
        registeredModules.Add(module);
    }
    return services;
}
```

### Option 5: Add Module Base Class (Optional Convenience)

```csharp
/// <summary>
/// Base class for modules with common functionality
/// </summary>
public abstract class ModuleBase : IModule
{
    public abstract string RoutePrefix { get; }
    
    public virtual IServiceCollection RegisterModule(IServiceCollection services)
    {
        // Default: no services to register
        return services;
    }

    public abstract IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>
    /// Helper method to create a route group for the module
    /// </summary>
    protected RouteGroupBuilder CreateGroup(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup(RoutePrefix)
            .WithTags(GetType().Name.Replace("Module", ""));
    }
}
```

## Example Module Implementations

### Basic Example: Order Module

```csharp
public class OrderModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services)
    {
        // Register Order-specific services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var orders = endpoints.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // CRUD Endpoints
        orders.MapGet("/", GetAllOrders)
            .WithName("GetAllOrders")
            .Produces<IEnumerable<OrderDto>>(StatusCodes.Status200OK);

        orders.MapGet("/{id:int}", GetOrder)
            .WithName("GetOrder")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        orders.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        orders.MapPut("/{id:int}", UpdateOrder)
            .WithName("UpdateOrder")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        orders.MapDelete("/{id:int}", DeleteOrder)
            .WithName("DeleteOrder")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // Additional operations
        orders.MapPost("/{id:int}/cancel", CancelOrder)
            .WithName("CancelOrder");

        orders.MapGet("/{id:int}/status", GetOrderStatus)
            .WithName("GetOrderStatus");

        return endpoints;
    }

    // Handler methods
    private static async Task<IResult> GetAllOrders(IOrderService service)
    {
        var orders = await service.GetAllOrdersAsync();
        return Results.Ok(orders);
    }

    private static async Task<IResult> GetOrder(int id, IOrderService service)
    {
        var order = await service.GetOrderByIdAsync(id);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderDto dto, 
        IOrderService service)
    {
        var order = await service.CreateOrderAsync(dto);
        return Results.Created($"/api/orders/{order.Id}", order);
    }

    private static async Task<IResult> UpdateOrder(
        int id, 
        UpdateOrderDto dto, 
        IOrderService service)
    {
        var order = await service.UpdateOrderAsync(id, dto);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }

    private static async Task<IResult> DeleteOrder(int id, IOrderService service)
    {
        var deleted = await service.DeleteOrderAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> CancelOrder(int id, IOrderService service)
    {
        var result = await service.CancelOrderAsync(id);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> GetOrderStatus(int id, IOrderService service)
    {
        var status = await service.GetOrderStatusAsync(id);
        return status is not null ? Results.Ok(status) : Results.NotFound();
    }
}
```

### Advanced Example: User Module with Authentication

```csharp
public class UserModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        // Public endpoints (no auth)
        users.MapPost("/register", Register)
            .AllowAnonymous();

        users.MapPost("/login", Login)
            .AllowAnonymous();

        // Protected endpoints (require auth)
        var authenticated = users.MapGroup("")
            .RequireAuthorization();

        authenticated.MapGet("/me", GetCurrentUser);
        authenticated.MapPut("/me", UpdateCurrentUser);
        authenticated.MapPost("/me/change-password", ChangePassword);

        // Admin-only endpoints
        var admin = users.MapGroup("")
            .RequireAuthorization("AdminPolicy");

        admin.MapGet("/", GetAllUsers);
        admin.MapGet("/{id:int}", GetUserById);
        admin.MapDelete("/{id:int}", DeleteUser);

        return endpoints;
    }

    private static async Task<IResult> Register(
        RegisterDto dto, 
        IUserService service)
    {
        var user = await service.RegisterAsync(dto);
        return Results.Created($"/api/users/{user.Id}", user);
    }

    private static async Task<IResult> Login(
        LoginDto dto, 
        IUserService service)
    {
        var token = await service.LoginAsync(dto);
        return token is not null 
            ? Results.Ok(new { token }) 
            : Results.Unauthorized();
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal user, 
        IUserService service)
    {
        var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var userDto = await service.GetUserByIdAsync(userId);
        return Results.Ok(userDto);
    }

    private static async Task<IResult> UpdateCurrentUser(
        ClaimsPrincipal user,
        UpdateUserDto dto,
        IUserService service)
    {
        var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var updated = await service.UpdateUserAsync(userId, dto);
        return Results.Ok(updated);
    }

    private static async Task<IResult> ChangePassword(
        ClaimsPrincipal user,
        ChangePasswordDto dto,
        IUserService service)
    {
        var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var success = await service.ChangePasswordAsync(userId, dto);
        return success ? Results.Ok() : Results.BadRequest();
    }

    private static async Task<IResult> GetAllUsers(IUserService service)
    {
        var users = await service.GetAllUsersAsync();
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserById(int id, IUserService service)
    {
        var user = await service.GetUserByIdAsync(id);
        return user is not null ? Results.Ok(user) : Results.NotFound();
    }

    private static async Task<IResult> DeleteUser(int id, IUserService service)
    {
        var deleted = await service.DeleteUserAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}
```

### Example with Handler Classes (Cleaner for Complex Logic)

```csharp
public class PaymentModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentProcessor, StripePaymentProcessor>();
        
        // Register handlers
        services.AddScoped<ProcessPaymentHandler>();
        services.AddScoped<RefundPaymentHandler>();
        
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var payments = endpoints.MapGroup("/api/payments")
            .WithTags("Payments")
            .RequireAuthorization();

        payments.MapPost("/process", 
            async (ProcessPaymentRequest req, ProcessPaymentHandler handler) => 
                await handler.HandleAsync(req));

        payments.MapPost("/{id:int}/refund", 
            async (int id, RefundPaymentHandler handler) => 
                await handler.HandleAsync(id));

        payments.MapGet("/{id:int}", GetPayment);
        payments.MapGet("/transactions", GetTransactions);

        return endpoints;
    }

    private static async Task<IResult> GetPayment(
        int id, 
        IPaymentService service)
    {
        var payment = await service.GetPaymentAsync(id);
        return payment is not null ? Results.Ok(payment) : Results.NotFound();
    }

    private static async Task<IResult> GetTransactions(
        IPaymentService service,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var transactions = await service.GetTransactionsAsync(from, to);
        return Results.Ok(transactions);
    }
}

// Separate handler classes for complex logic
public class ProcessPaymentHandler
{
    private readonly IPaymentService _service;
    private readonly ILogger<ProcessPaymentHandler> _logger;

    public ProcessPaymentHandler(
        IPaymentService service, 
        ILogger<ProcessPaymentHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IResult> HandleAsync(ProcessPaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId}", 
                request.OrderId);

            var result = await _service.ProcessPaymentAsync(request);
            
            if (result.Success)
            {
                return Results.Ok(result);
            }

            return Results.BadRequest(new { result.ErrorMessage });
        }
        catch (PaymentException ex)
        {
            _logger.LogError(ex, "Payment processing failed");
            return Results.Problem(ex.Message, statusCode: 402);
        }
    }
}
```

## Usage in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register all modules (discovers and registers services)
builder.Services.RegisterModules();

// Add other services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddApplicationInsights();

var app = builder.Build();

// Add middleware (your obfuscation middleware!)
app.UseObfuscationMiddleware();
app.UseAuthentication();
app.UseAuthorization();

// Map all module endpoints
app.MapEndpoints();

app.Run();
```

## Pros of This Pattern

✅ **Organization**: Each module is self-contained (services + endpoints)
✅ **Scalability**: Easy to add new modules without touching Program.cs
✅ **Testing**: Modules can be tested independently
✅ **Team collaboration**: Different teams can own different modules
✅ **Clear boundaries**: Natural split points if you need to extract to separate services
✅ **Minimal APIs**: Keeps Minimal API benefits (lightweight, fast)
✅ **Discoverability**: Auto-discovery means just create a class implementing IModule

## Cons to Consider

⚠️ **Reflection overhead**: Module discovery uses reflection (one-time cost at startup)
⚠️ **Less explicit**: Modules are discovered automatically (could be confusing for new devs)
⚠️ **Debugging**: Slightly harder to debug than explicit registration

## Best Practices

1. **One domain per module** (OrderModule, UserModule, not CrudModule)
2. **Keep handlers close to endpoints** (in same class or nearby)
3. **Use route groups** for consistent prefixes and tags
4. **Add OpenAPI/Swagger tags** for documentation
5. **Consider extracting complex logic** to handler classes
6. **Use the module pattern** to practice for potential microservice extraction later

## When to Split a Module to a Separate Service

When a module grows to 15-25 endpoints, or when you hit organizational boundaries (different teams, scaling needs), refer to `/docs/microservice-split-criteria.md` for guidance.

---

This pattern works great for your middleware-focused project because:
- ✅ Middleware runs globally (before modules)
- ✅ Modules organize endpoints cleanly
- ✅ Easy to extract to microservices later
- ✅ Maintains all Minimal API performance benefits
