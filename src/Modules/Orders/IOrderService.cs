namespace AzureAppServiceLoggingMiddleware.Modules.Orders;

using AzureAppServiceLoggingMiddleware.Modules.Orders.Models;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto);
    Task<bool> DeleteOrderAsync(int id);
    Task<bool> CancelOrderAsync(int id);
    Task<OrderStatus?> GetOrderStatusAsync(int id);
}
