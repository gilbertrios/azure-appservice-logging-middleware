namespace AzureAppServiceLoggingMiddleware.Modules.Payments;

using AzureAppServiceLoggingMiddleware.Modules.Payments.Models;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
    Task<PaymentDto?> GetPaymentByIdAsync(int id);
    Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId);
    Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto dto);
    Task<PaymentDto?> RefundPaymentAsync(int id, RefundPaymentDto dto);
    Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);
}
