namespace AzureAppServiceLoggingMiddleware.Modules.Payments;

using AzureAppServiceLoggingMiddleware.Modules.Payments.Models;

public class PaymentService : IPaymentService
{
    private static readonly List<PaymentDto> _payments = new()
    {
        new PaymentDto(1, 1, 299.99m, PaymentMethod.CreditCard, PaymentStatus.Completed, 
            DateTime.UtcNow.AddDays(-5), "TXN-001"),
        new PaymentDto(2, 2, 149.50m, PaymentMethod.PayPal, PaymentStatus.Completed, 
            DateTime.UtcNow.AddDays(-2), "TXN-002"),
        new PaymentDto(3, 3, 599.00m, PaymentMethod.CreditCard, PaymentStatus.Pending, 
            DateTime.UtcNow.AddDays(-1), null)
    };

    private static int _nextId = 4;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
    {
        _logger.LogInformation("Retrieving all payments");
        return Task.FromResult<IEnumerable<PaymentDto>>(_payments);
    }

    public Task<PaymentDto?> GetPaymentByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving payment with ID: {PaymentId}", id);
        var payment = _payments.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(payment);
    }

    public Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId)
    {
        _logger.LogInformation("Retrieving payment for order ID: {OrderId}", orderId);
        var payment = _payments.FirstOrDefault(p => p.OrderId == orderId);
        return Task.FromResult(payment);
    }

    public Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto dto)
    {
        _logger.LogInformation("Processing payment for order ID: {OrderId}, Amount: {Amount}", 
            dto.OrderId, dto.Amount);

        // Simulate payment processing
        var isSuccessful = Random.Shared.Next(1, 100) > 10; // 90% success rate
        var status = isSuccessful ? PaymentStatus.Completed : PaymentStatus.Failed;
        var transactionId = isSuccessful ? $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}" : null;

        var payment = new PaymentDto(
            _nextId++,
            dto.OrderId,
            dto.Amount,
            dto.Method,
            status,
            DateTime.UtcNow,
            transactionId
        );

        _payments.Add(payment);

        if (isSuccessful)
        {
            _logger.LogInformation("Payment processed successfully. Transaction ID: {TransactionId}", 
                transactionId);
        }
        else
        {
            _logger.LogWarning("Payment processing failed for order ID: {OrderId}", dto.OrderId);
        }

        return Task.FromResult(payment);
    }

    public Task<PaymentDto?> RefundPaymentAsync(int id, RefundPaymentDto dto)
    {
        _logger.LogInformation("Processing refund for payment ID: {PaymentId}, Reason: {Reason}", 
            id, dto.Reason);

        var payment = _payments.FirstOrDefault(p => p.Id == id);
        if (payment is null)
            return Task.FromResult<PaymentDto?>(null);

        if (payment.Status != PaymentStatus.Completed)
        {
            _logger.LogWarning("Cannot refund payment with status: {Status}", payment.Status);
            return Task.FromResult<PaymentDto?>(null);
        }

        var refundAmount = dto.RefundAmount ?? payment.Amount;
        var refundedPayment = payment with 
        { 
            Status = PaymentStatus.Refunded,
            Amount = refundAmount
        };

        _payments.Remove(payment);
        _payments.Add(refundedPayment);

        _logger.LogInformation("Refund processed successfully for payment ID: {PaymentId}", id);
        return Task.FromResult<PaymentDto?>(refundedPayment);
    }

    public Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
    {
        _logger.LogInformation("Retrieving payments from {From} to {To}", from, to);
        
        var payments = _payments
            .Where(p => p.ProcessedDate >= from && p.ProcessedDate <= to)
            .ToList();

        return Task.FromResult<IEnumerable<PaymentDto>>(payments);
    }
}
