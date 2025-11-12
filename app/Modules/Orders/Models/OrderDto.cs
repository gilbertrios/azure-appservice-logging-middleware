namespace AzureAppServiceLoggingMiddleware.Modules.Orders.Models;

public record OrderDto(
    int Id,
    string CustomerName,
    DateTime OrderDate,
    decimal TotalAmount,
    OrderStatus Status
);

public record CreateOrderDto(
    string CustomerName,
    decimal TotalAmount,
    List<OrderItemDto> Items
);

public record UpdateOrderDto(
    string CustomerName,
    decimal TotalAmount,
    OrderStatus Status
);

public record OrderItemDto(
    string ProductName,
    int Quantity,
    decimal Price
);

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}
