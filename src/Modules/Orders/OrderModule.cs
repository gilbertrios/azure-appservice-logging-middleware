namespace AzureAppServiceLoggingMiddleware.Modules.Orders;

using AzureAppServiceLoggingMiddleware.Infrastructure;
using AzureAppServiceLoggingMiddleware.Modules.Orders.Models;

public class OrderModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services)
    {
        // Register Order-specific services
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var orders = endpoints.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // GET: Get all orders
        orders.MapGet("/", GetAllOrders)
            .WithName("GetAllOrders")
            .WithSummary("Get all orders")
            .Produces<IEnumerable<OrderDto>>(StatusCodes.Status200OK);

        // GET: Get order by ID
        orders.MapGet("/{id:int}", GetOrderById)
            .WithName("GetOrder")
            .WithSummary("Get an order by ID")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: Create new order
        orders.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // PUT: Update order
        orders.MapPut("/{id:int}", UpdateOrder)
            .WithName("UpdateOrder")
            .WithSummary("Update an existing order")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE: Delete order
        orders.MapDelete("/{id:int}", DeleteOrder)
            .WithName("DeleteOrder")
            .WithSummary("Delete an order")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // POST: Cancel order
        orders.MapPost("/{id:int}/cancel", CancelOrder)
            .WithName("CancelOrder")
            .WithSummary("Cancel an order");

        // GET: Get order status
        orders.MapGet("/{id:int}/status", GetOrderStatus)
            .WithName("GetOrderStatus")
            .WithSummary("Get the status of an order");

        return endpoints;
    }

    private static async Task<IResult> GetAllOrders(IOrderService service)
    {
        var orders = await service.GetAllOrdersAsync();
        return Results.Ok(orders);
    }

    private static async Task<IResult> GetOrderById(int id, IOrderService service)
    {
        var order = await service.GetOrderByIdAsync(id);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }

    private static async Task<IResult> CreateOrder(CreateOrderDto dto, IOrderService service)
    {
        var order = await service.CreateOrderAsync(dto);
        return Results.Created($"/api/orders/{order.Id}", order);
    }

    private static async Task<IResult> UpdateOrder(int id, UpdateOrderDto dto, IOrderService service)
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
        var cancelled = await service.CancelOrderAsync(id);
        return cancelled ? Results.Ok(new { message = "Order cancelled successfully" }) : Results.NotFound();
    }

    private static async Task<IResult> GetOrderStatus(int id, IOrderService service)
    {
        var status = await service.GetOrderStatusAsync(id);
        return status is not null 
            ? Results.Ok(new { orderId = id, status = status.ToString() }) 
            : Results.NotFound();
    }
}
