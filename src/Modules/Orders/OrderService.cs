namespace AzureAppServiceLoggingMiddleware.Modules.Orders;

using AzureAppServiceLoggingMiddleware.Modules.Orders.Models;

public class OrderService : IOrderService
{
    private static readonly List<OrderDto> _orders = new()
    {
        new OrderDto(1, "John Doe", DateTime.UtcNow.AddDays(-5), 299.99m, OrderStatus.Completed),
        new OrderDto(2, "Jane Smith", DateTime.UtcNow.AddDays(-2), 149.50m, OrderStatus.Processing),
        new OrderDto(3, "Bob Johnson", DateTime.UtcNow.AddDays(-1), 599.00m, OrderStatus.Pending)
    };

    private static int _nextId = 4;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        _logger.LogInformation("Retrieving all orders");
        return Task.FromResult<IEnumerable<OrderDto>>(_orders);
    }

    public Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving order with ID: {OrderId}", id);
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return Task.FromResult(order);
    }

    public Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        _logger.LogInformation("Creating new order for customer: {CustomerName}", dto.CustomerName);
        
        var order = new OrderDto(
            _nextId++,
            dto.CustomerName,
            DateTime.UtcNow,
            dto.TotalAmount,
            OrderStatus.Pending
        );
        
        _orders.Add(order);
        return Task.FromResult(order);
    }

    public Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto)
    {
        _logger.LogInformation("Updating order with ID: {OrderId}", id);
        
        var existingOrder = _orders.FirstOrDefault(o => o.Id == id);
        if (existingOrder is null)
            return Task.FromResult<OrderDto?>(null);

        var updatedOrder = existingOrder with
        {
            CustomerName = dto.CustomerName,
            TotalAmount = dto.TotalAmount,
            Status = dto.Status
        };

        _orders.Remove(existingOrder);
        _orders.Add(updatedOrder);
        
        return Task.FromResult<OrderDto?>(updatedOrder);
    }

    public Task<bool> DeleteOrderAsync(int id)
    {
        _logger.LogInformation("Deleting order with ID: {OrderId}", id);
        
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order is null)
            return Task.FromResult(false);

        _orders.Remove(order);
        return Task.FromResult(true);
    }

    public Task<bool> CancelOrderAsync(int id)
    {
        _logger.LogInformation("Cancelling order with ID: {OrderId}", id);
        
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order is null)
            return Task.FromResult(false);

        var cancelledOrder = order with { Status = OrderStatus.Cancelled };
        _orders.Remove(order);
        _orders.Add(cancelledOrder);
        
        return Task.FromResult(true);
    }

    public Task<OrderStatus?> GetOrderStatusAsync(int id)
    {
        _logger.LogInformation("Retrieving status for order ID: {OrderId}", id);
        
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return Task.FromResult(order?.Status);
    }
}
