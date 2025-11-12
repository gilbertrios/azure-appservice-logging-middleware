namespace AzureAppServiceLoggingMiddleware.Modules.Payments;

using AzureAppServiceLoggingMiddleware.Infrastructure;
using AzureAppServiceLoggingMiddleware.Modules.Payments.Models;

public class PaymentModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services)
    {
        // Register Payment-specific services
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var payments = endpoints.MapGroup("/api/payments")
            .WithTags("Payments")
            .WithOpenApi();

        // GET: Get all payments
        payments.MapGet("/", GetAllPayments)
            .WithName("GetAllPayments")
            .WithSummary("Get all payments")
            .Produces<IEnumerable<PaymentDto>>(StatusCodes.Status200OK);

        // GET: Get payment by ID
        payments.MapGet("/{id:int}", GetPaymentById)
            .WithName("GetPayment")
            .WithSummary("Get a payment by ID")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // GET: Get payment by order ID
        payments.MapGet("/order/{orderId:int}", GetPaymentByOrderId)
            .WithName("GetPaymentByOrder")
            .WithSummary("Get payment by order ID")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: Process payment
        payments.MapPost("/process", ProcessPayment)
            .WithName("ProcessPayment")
            .WithSummary("Process a new payment")
            .Produces<PaymentDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // POST: Refund payment
        payments.MapPost("/{id:int}/refund", RefundPayment)
            .WithName("RefundPayment")
            .WithSummary("Refund a payment")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // GET: Get payments by date range
        payments.MapGet("/transactions", GetPaymentsByDateRange)
            .WithName("GetTransactions")
            .WithSummary("Get payments within a date range")
            .Produces<IEnumerable<PaymentDto>>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<IResult> GetAllPayments(IPaymentService service)
    {
        var payments = await service.GetAllPaymentsAsync();
        return Results.Ok(payments);
    }

    private static async Task<IResult> GetPaymentById(int id, IPaymentService service)
    {
        var payment = await service.GetPaymentByIdAsync(id);
        return payment is not null ? Results.Ok(payment) : Results.NotFound();
    }

    private static async Task<IResult> GetPaymentByOrderId(int orderId, IPaymentService service)
    {
        var payment = await service.GetPaymentByOrderIdAsync(orderId);
        return payment is not null 
            ? Results.Ok(payment) 
            : Results.NotFound(new { message = $"No payment found for order {orderId}" });
    }

    private static async Task<IResult> ProcessPayment(ProcessPaymentDto dto, IPaymentService service)
    {
        var payment = await service.ProcessPaymentAsync(dto);
        
        if (payment.Status == PaymentStatus.Completed)
        {
            return Results.Created($"/api/payments/{payment.Id}", payment);
        }

        return Results.BadRequest(new 
        { 
            message = "Payment processing failed",
            payment
        });
    }

    private static async Task<IResult> RefundPayment(
        int id, 
        RefundPaymentDto dto, 
        IPaymentService service)
    {
        var payment = await service.RefundPaymentAsync(id, dto);
        
        if (payment is null)
        {
            return Results.NotFound(new { message = "Payment not found or cannot be refunded" });
        }

        return Results.Ok(new
        {
            message = "Refund processed successfully",
            payment
        });
    }

    private static async Task<IResult> GetPaymentsByDateRange(
        IPaymentService service,
        DateTime? from = null,
        DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;

        var payments = await service.GetPaymentsByDateRangeAsync(fromDate, toDate);
        
        return Results.Ok(new
        {
            from = fromDate,
            to = toDate,
            count = payments.Count(),
            payments
        });
    }
}
