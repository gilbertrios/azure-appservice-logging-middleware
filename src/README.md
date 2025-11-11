# Running the Minimal API

## Quick Start

```bash
# Navigate to the src directory
cd /Users/gilbertrios/repos/azure-appservice-logging-middleware/src

# Restore packages
dotnet restore

# Run the application
dotnet run
```

The API will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## Project Structure

```
/src
  /Infrastructure
    - IModule.cs              # Module interface
    - ModuleExtensions.cs     # Auto-discovery extensions
  /Modules
    /Orders
      - OrderModule.cs        # Order endpoints (7 endpoints)
      - OrderService.cs       # Order business logic
      - IOrderService.cs      # Order service interface
      /Models
        - OrderDto.cs         # Order DTOs and enums
    /Payments
      - PaymentModule.cs      # Payment endpoints (6 endpoints)
      - PaymentService.cs     # Payment business logic
      - IPaymentService.cs    # Payment service interface
      /Models
        - PaymentDto.cs       # Payment DTOs and enums
  - Program.cs                # Application entry point
  - AzureAppServiceLoggingMiddleware.csproj
```

## Available Endpoints

### Orders Module (7 endpoints)

```
GET    /api/orders              - Get all orders
GET    /api/orders/{id}         - Get order by ID
POST   /api/orders              - Create new order
PUT    /api/orders/{id}         - Update order
DELETE /api/orders/{id}         - Delete order
POST   /api/orders/{id}/cancel  - Cancel order
GET    /api/orders/{id}/status  - Get order status
```

### Payments Module (6 endpoints)

```
GET    /api/payments                  - Get all payments
GET    /api/payments/{id}             - Get payment by ID
GET    /api/payments/order/{orderId}  - Get payment by order ID
POST   /api/payments/process          - Process new payment
POST   /api/payments/{id}/refund      - Refund payment
GET    /api/payments/transactions     - Get payments by date range
```

### Health Check

```
GET    /health                   - API health check
```

## Example API Calls

### Create Order
```bash
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "totalAmount": 299.99,
    "items": [
      {
        "productName": "Laptop",
        "quantity": 1,
        "price": 299.99
      }
    ]
  }'
```

### Get All Orders
```bash
curl https://localhost:5001/api/orders
```

### Process Payment
```bash
curl -X POST https://localhost:5001/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "amount": 299.99,
    "method": "CreditCard",
    "cardNumber": "4111111111111111",
    "cardHolderName": "John Doe",
    "expiryDate": "12/25",
    "cvv": "123"
  }'
```

### Get Payment by Order
```bash
curl https://localhost:5001/api/payments/order/1
```

### Cancel Order
```bash
curl -X POST https://localhost:5001/api/orders/1/cancel
```

### Refund Payment
```bash
curl -X POST https://localhost:5001/api/payments/1/refund \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Customer request",
    "refundAmount": 299.99
  }'
```

## Features

✅ **Module Auto-Discovery** - Modules are automatically discovered and registered
✅ **Clean Architecture** - Each module is self-contained with models, services, and endpoints
✅ **Swagger/OpenAPI** - Full API documentation at /swagger
✅ **In-Memory Storage** - Pre-populated with sample data (ready for real DB)
✅ **Minimal APIs** - Lightweight, fast, modern .NET 8
✅ **Logging** - Built-in logging for all operations
✅ **Type Safety** - Records and strong typing throughout

## Next Steps

### Add Your Obfuscation Middleware

1. Update `Program.cs` to use your middleware:
```csharp
// After app.Build()
app.UseObfuscationMiddleware(); // Add this line
app.UseHttpsRedirection();
app.MapEndpoints();
```

### Add Database

Replace in-memory storage with real database:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Add Authentication

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Update endpoints:
```csharp
payments.MapPost("/process", ProcessPayment)
    .RequireAuthorization(); // Add authentication
```

## Testing with Swagger

1. Run the application: `dotnet run`
2. Open browser: `https://localhost:5001/swagger`
3. Try out the endpoints interactively

## Architecture Benefits

This structure is ready for:
- ✅ **Blue/Green Deployments** - Each module can be tested independently
- ✅ **Microservice Extraction** - Easy to move Orders or Payments to separate service
- ✅ **Team Ownership** - Different teams can own different modules
- ✅ **Middleware Integration** - Obfuscation middleware will work across all endpoints
- ✅ **Scaling** - Can scale modules independently when extracted

## Module Pattern Benefits

Each module (Orders, Payments) is:
- **Self-contained** - All code for the domain in one folder
- **Testable** - Can be tested independently
- **Discoverable** - Auto-registered via reflection
- **Extractable** - Clear boundary for microservice split
