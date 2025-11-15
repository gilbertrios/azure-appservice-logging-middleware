# Testing Guide

This document provides comprehensive information about testing the Azure App Service Logging Middleware application.

## Overview

The project includes two types of tests:
- **Unit Tests** - Fast, isolated tests for individual components
- **Integration Tests** - End-to-end tests through the full middleware pipeline

Both test suites run automatically in CI/CD and can be executed locally during development.

## Quick Commands

```bash
# Run all tests
dotnet test

# Run only unit tests (fast)
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run with detailed output
dotnet test --verbosity normal

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~ObfuscationMiddlewareTests"
```

## Test Projects

### Unit Tests

**Location:** `tests/AzureAppServiceLoggingMiddleware.UnitTests/`

**Purpose:** Test individual components in isolation with mocked dependencies.

**What's tested:**
- ObfuscationMiddleware logic
- JSON parsing and traversal
- Sensitive property detection
- Edge cases and error handling
- Configuration options

**Characteristics:**
- ✅ Fast execution (~100ms total)
- ✅ No external dependencies
- ✅ Mock ILogger, HttpContext, etc.
- ✅ High code coverage target (>80%)

**Example test structure:**

```csharp
[TestCategory("Unit")]
public class ObfuscationMiddlewareTests
{
    private readonly Mock<ILogger<ObfuscationMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;
    
    [TestMethod]
    public async Task Middleware_ObfuscatesCreditCardInRequest()
    {
        // Arrange - Set up test data and mocks
        var context = CreateHttpContext(requestBody: "{\"creditCard\":\"1234-5678-9012-3456\"}");
        
        // Act - Execute middleware
        await _middleware.InvokeAsync(context);
        
        // Assert - Verify sensitive data was obfuscated
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("***REDACTED***")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
```

**Test categories covered:**

1. **Obfuscation Logic**
   - Credit card numbers
   - CVV codes
   - Passwords
   - API keys and tokens
   - SSN and sensitive identifiers

2. **JSON Handling**
   - Nested objects
   - Arrays
   - Mixed structures
   - Invalid JSON
   - Null/empty bodies

3. **Configuration**
   - Custom obfuscation masks
   - Sensitive property lists
   - Enable/disable functionality
   - Case-insensitive matching

4. **Edge Cases**
   - Missing properties
   - Circular references
   - Large payloads
   - Unicode characters
   - Special characters in property names

### Integration Tests

**Location:** `tests/AzureAppServiceLoggingMiddleware.IntegrationTests/`

**Purpose:** Test the full application stack with real HTTP requests.

**What's tested:**
- Complete middleware pipeline
- Module registration and discovery
- API endpoint functionality
- Obfuscation in real request/response flow
- Health check endpoints
- Error handling and status codes

**Characteristics:**
- ⚠️ Slower execution (~500ms total)
- ✅ Uses WebApplicationFactory
- ✅ Real HTTP requests (in-memory)
- ✅ Tests actual application behavior

**Example test structure:**

```csharp
[TestCategory("Integration")]
public class ObfuscationMiddlewareIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [TestMethod]
    public async Task PaymentEndpoint_ObfuscatesSensitiveData()
    {
        // Arrange
        var payment = new
        {
            orderId = 1,
            amount = 299.99,
            creditCard = "1234-5678-9012-3456",
            cvv = "123"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/payments/process", payment);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        // Verify logs show obfuscated data (via test logger)
    }
}
```

**Test scenarios covered:**

1. **Orders Module**
   - Create order
   - Get order by ID
   - Update order
   - Delete order
   - Cancel order
   - Get order status

2. **Payments Module**
   - Process payment with sensitive data
   - Refund payment
   - Get payment by order
   - List transactions

3. **Middleware Integration**
   - Request body obfuscation
   - Response body obfuscation
   - Headers handling
   - Error responses

4. **Health Checks**
   - `/health` endpoint
   - Application responsiveness
   - Dependency checks

## Test Execution

### Local Development

Run tests during development to ensure changes don't break functionality:

```bash
# Quick feedback loop
dotnet test --filter "Category=Unit"

# Before committing
dotnet test

# With coverage report
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport
```

### CI/CD Pipeline

Tests run automatically in GitHub Actions:

**On Pull Requests:**
- ✅ All tests must pass before merge
- ✅ Test results published to PR
- ✅ Failed tests show inline annotations

**On Main Branch:**
- ✅ Tests run before deployment
- ✅ Deployment blocked if tests fail
- ✅ Test results archived as artifacts

**Workflow configuration:**

```yaml
- name: Run Tests
  run: dotnet test --configuration Release --logger "trx;LogFileName=test-results.trx"
  
- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Test Results
    path: '**/test-results.trx'
    reporter: dotnet-trx
```

## Manual Testing with cURL

Test the API manually to verify obfuscation in action:

### Create an Order

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

**Expected Response:**
```json
{
  "id": 1,
  "customerName": "John Doe",
  "totalAmount": 299.99,
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Check Logs:** Request/response logged without obfuscation (no sensitive data)

### Process Payment with Sensitive Data

```bash
curl -X POST https://localhost:5001/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "amount": 299.99,
    "method": "CreditCard",
    "creditCard": "1234-5678-9012-3456",
    "cvv": "123",
    "token": "secret-api-key-12345"
  }'
```

**Expected Response:**
```json
{
  "id": 1,
  "orderId": 1,
  "amount": 299.99,
  "status": "Completed",
  "transactionId": "TXN-123456"
}
```

**Check Logs:** Should show obfuscated data:
```json
{
  "orderId": 1,
  "amount": 299.99,
  "method": "CreditCard",
  "creditCard": "***REDACTED***",
  "cvv": "***REDACTED***",
  "token": "***REDACTED***"
}
```

### Get Payment Details

```bash
curl -X GET https://localhost:5001/api/payments/1 \
  -H "Content-Type: application/json"
```

### Cancel Order

```bash
curl -X POST https://localhost:5001/api/orders/1/cancel \
  -H "Content-Type: application/json"
```

### Health Check

```bash
curl -X GET https://localhost:5001/health
```

**Expected Response:**
```json
{
  "status": "Healthy"
}
```

## Testing with Swagger UI

Interactive API testing through the built-in Swagger interface.

### Access Swagger UI

1. Start the application:
   ```bash
   cd app
   dotnet run
   ```

2. Navigate to: `https://localhost:5001/swagger`

### Test Workflow

1. **Expand an endpoint** (e.g., POST /api/payments/process)

2. **Click "Try it out"**

3. **Enter test data:**
   ```json
   {
     "orderId": 1,
     "amount": 299.99,
     "method": "CreditCard",
     "creditCard": "1234-5678-9012-3456",
     "cvv": "123"
   }
   ```

4. **Click "Execute"**

5. **Check response** - API returns actual data (unobfuscated)

6. **Check console logs** - Logs show obfuscated data:
   ```
   Request Body (Obfuscated): {"orderId":1,"amount":299.99,"method":"CreditCard","creditCard":"***REDACTED***","cvv":"***REDACTED***"}
   ```

### Verify Obfuscation

Open terminal where `dotnet run` is executing and look for log entries:

```
info: AzureAppServiceLoggingMiddleware.Middleware.ObfuscationMiddleware[0]
      Request: POST /api/payments/process
      Obfuscated Body: {"orderId":1,"creditCard":"***REDACTED***","cvv":"***REDACTED***"}
```

## Test Data

### Sample Orders

```json
{
  "customerName": "Jane Smith",
  "totalAmount": 599.99,
  "items": [
    {"productName": "Monitor", "quantity": 2, "price": 299.99}
  ]
}
```

### Sample Payments

```json
{
  "orderId": 1,
  "amount": 599.99,
  "method": "CreditCard",
  "creditCard": "4532-1234-5678-9010",
  "cvv": "456",
  "expiryDate": "12/25"
}
```

### Sensitive Data Test Cases

Test various sensitive property names:

```json
{
  "password": "MySecret123",
  "api_key": "sk_test_123456",
  "apiKey": "pk_live_789012",
  "token": "Bearer abc123def456",
  "creditCardNumber": "4532123456789010",
  "ssn": "123-45-6789",
  "Authorization": "Basic dXNlcjpwYXNz"
}
```

All should be obfuscated in logs.

## Code Coverage

### Generate Coverage Report

```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"

# Install report generator (one time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report
open coveragereport/index.html  # macOS
start coveragereport/index.html # Windows
```

### Coverage Targets

| Component | Target | Current |
|-----------|--------|---------|
| ObfuscationMiddleware | >90% | ~95% |
| Module Infrastructure | >80% | ~85% |
| API Endpoints | >70% | ~75% |
| Overall | >80% | ~85% |

### Areas Not Covered

Acceptable exclusions:
- ❌ Program.cs (startup configuration)
- ❌ DTOs and models (data classes)
- ❌ Third-party integrations (mocked)

## Performance Testing

### Load Testing with Apache Bench

```bash
# Test health endpoint
ab -n 1000 -c 10 https://localhost:5001/health

# Test API endpoint
ab -n 1000 -c 10 -p payment.json -T application/json \
   https://localhost:5001/api/payments/process
```

**Benchmarks (Development Machine):**
- Health endpoint: ~2000 req/sec
- Payment endpoint: ~800 req/sec
- Middleware overhead: <1ms per request

### Stress Testing

```bash
# Install k6 (one time)
brew install k6  # macOS
choco install k6 # Windows

# Run load test
k6 run loadtest.js
```

**loadtest.js:**
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '30s', target: 0 },
  ],
};

export default function () {
  let payload = JSON.stringify({
    orderId: 1,
    amount: 299.99,
    creditCard: "1234-5678-9012-3456",
    cvv: "123"
  });

  let response = http.post('https://localhost:5001/api/payments/process', payload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 200ms': (r) => r.timings.duration < 200,
  });

  sleep(1);
}
```

## Troubleshooting

### Tests Fail Locally

**Problem:** Tests pass in CI but fail locally

**Solutions:**
1. Ensure .NET SDK version matches CI (check `.github/workflows`)
2. Clean build artifacts: `dotnet clean && dotnet build`
3. Restore packages: `dotnet restore`
4. Check for port conflicts (5000, 5001)

### Integration Tests Timeout

**Problem:** Integration tests hang or timeout

**Solutions:**
1. Check if application starts: `dotnet run` manually
2. Verify no other instances running on same ports
3. Increase timeout in test initialization
4. Check for deadlocks in middleware

### Coverage Report Not Generated

**Problem:** Coverage files missing or empty

**Solutions:**
1. Install coverage collector: `dotnet tool install --global coverlet.console`
2. Ensure test project has coverage package
3. Check file paths in reportgenerator command
4. Verify tests actually ran (check for .trx files)

### Swagger UI Not Loading

**Problem:** 404 when accessing /swagger

**Solutions:**
1. Verify Swagger is configured in Program.cs
2. Check environment is Development
3. Ensure app is running: check console for startup logs
4. Try alternate URL: `/swagger/index.html`

## Best Practices

### Writing Tests

1. **Follow AAA Pattern**
   - Arrange: Set up test data
   - Act: Execute functionality
   - Assert: Verify results

2. **Use Descriptive Names**
   ```csharp
   // Good
   [TestMethod]
   public async Task Middleware_ObfuscatesCreditCard_WhenPresentInRequestBody()
   
   // Bad
   [TestMethod]
   public async Task Test1()
   ```

3. **Test One Thing**
   - Each test should verify one specific behavior
   - Multiple assertions are OK if testing same concept

4. **Avoid Test Interdependence**
   - Tests should run in any order
   - Use `[TestInitialize]` and `[TestCleanup]`

5. **Mock External Dependencies**
   - Don't call real APIs
   - Don't access real databases
   - Use in-memory alternatives for integration tests

### Test Maintenance

1. **Keep Tests Fast**
   - Unit tests should be < 100ms
   - Integration tests should be < 1s
   - Use `[Ignore]` for slow tests temporarily

2. **Update Tests with Code**
   - Failing tests after changes = update tests
   - Tests are documentation of behavior

3. **Review Test Coverage**
   - Check coverage reports regularly
   - Add tests for uncovered critical paths

4. **Clean Up Test Data**
   - Use `[TestCleanup]` to reset state
   - Don't leave test artifacts

## Related Documentation

- [Repository Structure](repository-structure.md) - Test project organization
- [CI/CD Pipeline](cicd-pipeline.md) - Automated test execution
- [Setup Guide](setup-guide.md) - Local development environment
- [Module Pattern](module-pattern.md) - Testing modules in isolation
