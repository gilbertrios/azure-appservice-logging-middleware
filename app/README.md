# Running the Minimal API

## Quick Start

```bash
# Navigate to the app directory
cd azure-appservice-logging-middleware/app

# Restore packages
dotnet restore

# Run the application
dotnet run
```

The API will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## Running with VS Code Debugger

Press **F5** or use the **Run and Debug** view. The launch configuration is already set up in `.vscode/launch.json`.

## Project Structure

```
/app
  /Infrastructure
    - IModule.cs              # Module interface
    - ModuleExtensions.cs     # Auto-discovery extensions
  /Middleware
    - ObfuscationMiddleware.cs           # Request/response obfuscation middleware
    - ObfuscationOptions.cs              # Configuration model
    - ObfuscationMiddlewareExtensions.cs # DI extensions
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
  /Properties
    - launchSettings.json     # Development launch settings
  - Program.cs                # Application entry point with middleware configured
  - appsettings.json          # Configuration including obfuscation settings
  - appsettings.Development.json  # Development-specific settings
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
curl http://localhost:5000/api/orders
```

### Process Payment (includes sensitive data for obfuscation testing)
```bash
curl -X POST http://localhost:5000/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "amount": 299.99,
    "method": "CreditCard",
    "creditCard": "1234-5678-9012-3456",
    "cvv": "123",
    "token": "secret-api-key"
  }'
```

**Note:** The `creditCard`, `cvv`, and `token` fields will be obfuscated in logs.

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

✅ **Obfuscation Middleware** - Automatically obfuscates sensitive data in request/response logs
✅ **Application Insights Integration** - Logs flow to Azure Application Insights with custom properties
✅ **Module Auto-Discovery** - Modules are automatically discovered and registered
✅ **Clean Architecture** - Each module is self-contained with models, services, and endpoints
✅ **Swagger/OpenAPI** - Full API documentation at /swagger
✅ **In-Memory Storage** - Pre-populated with sample data (ready for real DB)
✅ **Minimal APIs** - Lightweight, fast, modern .NET 9
✅ **Logging** - Built-in logging for all operations with obfuscated request/response bodies
✅ **Type Safety** - Records and strong typing throughout
✅ **Configurable Masking** - Control which properties to obfuscate via appsettings.json

## Obfuscation Middleware

The API includes a custom middleware that automatically obfuscates sensitive data in logs.

### How It Works

1. **Captures** request and response bodies without modifying them
2. **Recursively traverses** JSON to find sensitive properties
3. **Obfuscates** matching properties (case-insensitive)
4. **Logs** obfuscated versions to console and Application Insights

### Configuration

Edit `appsettings.json` to control which properties are obfuscated:

```json
{
  "ObfuscationMiddleware": {
    "Enabled": true,
    "ObfuscationMask": "***REDACTED***",
    "SensitiveProperties": [
      "password",
      "creditCard",
      "creditCardNumber",
      "cardNumber",
      "cvv",
      "ssn",
      "apiKey",
      "token",
      "authorization"
    ]
  }
}
```

### Test Obfuscation

```bash
# Send request with sensitive data
curl -X POST http://localhost:5000/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "amount": 59.99,
    "method": "CreditCard",
    "creditCard": "1234-5678-9012-3456",
    "cvv": "123",
    "token": "secret-token"
  }'
```

**Check the console logs** - you'll see:
```
Obfuscated Request: {"orderId":1,"amount":59.99,"creditCard":"***REDACTED***","cvv":"***REDACTED***","token":"***REDACTED***"}
```

### Application Insights

The middleware logs custom properties that flow to Application Insights:
- `RequestPath` - The endpoint called
- `RequestMethod` - HTTP method (POST, GET, etc.)
- `StatusCode` - Response status code
- `ObfuscatedRequest` - Request body with sensitive data masked
- `ObfuscatedResponse` - Response body with sensitive data masked

**To enable Application Insights:**
1. Create Application Insights resource in Azure
2. Copy connection string
3. Update `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=xxxxx;IngestionEndpoint=https://..."
  }
}
```

## Next Steps

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

## 🧪 Testing

### Run Tests

```bash
# Navigate to repository root
cd azure-appservice-logging-middleware

# Run all tests
dotnet test

# Run only unit tests (fast)
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"
```

### Test Projects

**Unit Tests** - `tests/AzureAppServiceLoggingMiddleware.UnitTests/`
- Tests middleware logic in isolation
- Mock dependencies (ILogger, HttpContext)
- Fast execution (~100ms for all tests)

**Integration Tests** - `tests/AzureAppServiceLoggingMiddleware.IntegrationTests/`
- End-to-end API testing
- Uses WebApplicationFactory
- Tests full middleware pipeline
- Slower execution (~500ms for all tests)

See [Testing Documentation](../README.md#-testing) for more details.
