namespace AzureAppServiceLoggingMiddleware.Modules.Payments.Models;

public record PaymentDto(
    int Id,
    int OrderId,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime ProcessedDate,
    string? TransactionId
);

public record ProcessPaymentDto(
    int OrderId,
    decimal Amount,
    PaymentMethod Method,
    string CardNumber,
    string CardHolderName,
    string ExpiryDate,
    string Cvv
);

public record RefundPaymentDto(
    string Reason,
    decimal? RefundAmount
);

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    PayPal,
    BankTransfer
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}
