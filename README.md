# Azure App Service Logging Middleware 🔒

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Azure](https://img.shields.io/badge/Azure-App%20Service-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/en-us/services/app-service/)
[![Application Insights](https://img.shields.io/badge/Application%20Insights-Enabled-00BCF2?logo=microsoft-azure)](https://azure.microsoft.com/en-us/services/monitor/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A production-ready ASP.NET Core minimal API showcasing **automatic sensitive data obfuscation** in logs with Azure Application Insights integration. Built with clean modular architecture for easy microservice extraction.

## 🎯 Key Features

- **🔐 Smart Obfuscation Middleware** - Automatically redacts sensitive data (credit cards, passwords, tokens) from logs before they reach Application Insights
- **☁️ Azure Application Insights Integration** - Seamless telemetry with custom properties and structured logging
- **🧩 Modular Architecture** - Self-contained modules (Orders, Payments) ready for microservice extraction
- **⚡ .NET 9 Minimal APIs** - Fast, lightweight, modern ASP.NET Core
- **📝 Auto-Discovery** - Modules automatically registered via reflection
- **🔧 Configurable** - Control obfuscation patterns via `appsettings.json`
- **📚 OpenAPI/Swagger** - Full API documentation out of the box
- **🚀 Production-Ready** - Includes health checks, structured logging, and comprehensive testing

## 🚀 Quick Start

### Run Application Locally

```bash
# Clone the repository
git clone https://github.com/gilbertrios/azure-appservice-logging-middleware.git
cd azure-appservice-logging-middleware/app

# Run the application
dotnet run
```

The API will be available at:
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

### Deploy to Azure

```bash
# 1. Configure Azure credentials (see docs/setup-guide.md)
# 2. Push to main branch
git push origin main

# The 7-stage pipeline will:
# ✅ Build application
# ✅ Provision infrastructure (Terraform)
# ✅ Deploy to green slot
# ✅ Run regression tests on green
# ✅ Swap to production
# ✅ Run smoke tests on production
# ✅ Auto rollback if smoke tests fail
```

## 🔒 Obfuscation Middleware in Action

The middleware automatically detects and obfuscates sensitive properties in request/response bodies:

### Example Request
```bash
curl -X POST http://localhost:5000/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "amount": 299.99,
    "creditCard": "1234-5678-9012-3456",
    "cvv": "123",
    "token": "secret-api-key"
  }'
```

### Console Output (Obfuscated)
```json
{
  "orderId": 1,
  "amount": 299.99,
  "creditCard": "***REDACTED***",
  "cvv": "***REDACTED***",
  "token": "***REDACTED***"
}
```

✅ **Actual API response remains unchanged** - only logs are obfuscated!

## 🏗️ Repository Architecture

```
azure-appservice-logging-middleware/
├── .github/
│   └── workflows/
│       ├── deploy-blue-green.yml     # 7-stage deployment pipeline (auto rollback)
│       ├── manual-rollback.yml       # On-demand rollback workflow
│       ├── ci-pr-validation.yml      # PR validation (build + terraform)
│       └── _build-app.yml            # Reusable build workflow
│
├── app/                              # .NET 9.0 Application
│   ├── Infrastructure/               # Module pattern implementation
│   ├── Middleware/                   # Obfuscation middleware
│   ├── Modules/                      # Orders & Payments modules
│   ├── Properties/                   # launchSettings.json
│   └── Program.cs
│
├── tests/                            # Test Projects
│   ├── AzureAppServiceLoggingMiddleware.UnitTests/
│   │   └── Middleware/
│   │       └── ObfuscationMiddlewareTests.cs
│   └── AzureAppServiceLoggingMiddleware.IntegrationTests/
│       └── ObfuscationMiddlewareIntegrationTests.cs
│
├── infrastructure/                   # Terraform IaC
│   ├── terraform/
│   │   ├── environments/
│   │   │   └── dev/                  # Dev environment config
│   │   └── modules/
│   │       └── app-service/          # App Service with slots
│   └── scripts/
│
└── docs/                            # Documentation
```

See [Repository Structure](docs/repository-structure.md) for detailed breakdown.

### Module Pattern Benefits

Each module is:
- **Self-contained** - All domain code in one folder
- **Testable** - Clear boundaries and interfaces
- **Discoverable** - Auto-registered via reflection
- **Extractable** - Ready for microservice split

## 📡 API Endpoints

### Orders Module
```
GET    /api/orders                  - List all orders
GET    /api/orders/{id}             - Get order by ID
POST   /api/orders                  - Create new order
PUT    /api/orders/{id}             - Update order
DELETE /api/orders/{id}             - Delete order
POST   /api/orders/{id}/cancel      - Cancel order
GET    /api/orders/{id}/status      - Get order status
```

### Payments Module
```
GET    /api/payments                       - List all payments
GET    /api/payments/{id}                  - Get payment by ID
GET    /api/payments/order/{orderId}       - Get payment for order
POST   /api/payments/process               - Process payment
POST   /api/payments/{id}/refund           - Refund payment
GET    /api/payments/transactions          - Get payments by date range
```

### Health Check
```
GET    /health                      - API health status
```

## ⚙️ Configuration

Edit `appsettings.json` to customize obfuscation behavior:

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
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=xxxxx;..."
  }
}
```

### Obfuscation Features

- ✅ **Case-insensitive** matching
- ✅ **Recursive** JSON traversal (handles nested objects/arrays)
- ✅ **Configurable** mask pattern
- ✅ **Zero performance impact** on actual API responses
- ✅ **Works with** Application Insights, console logs, and custom loggers

## ☁️ Azure Infrastructure

### Provisioned Resources (Dev Environment)

```
Resource Group: rg-logmw-dev
├── App Service Plan (Linux, S1)  # S1 required for deployment slots
├── App Service
│   ├── Production Slot (blue)
│   └── Green Slot (staging)
├── Application Insights
└── Log Analytics Workspace (7-day retention)
```

### Blue-Green Deployment Slots

- **Production Slot** - Current live version
- **Green Slot** - New version testing
- **Instant Swap** - Zero-downtime deployment
- **Quick Rollback** - Swap back if issues detected

### Deploy Infrastructure

```bash
cd infrastructure/terraform/environments/dev
terraform init
terraform apply
```

See [Infrastructure README](infrastructure/README.md) for details.

## 📊 Application Insights Integration

The middleware automatically logs obfuscated request/response data to Application Insights:

| Property | Description |
|----------|-------------|
| `RequestPath` | The endpoint called |
| `RequestMethod` | HTTP method (POST, GET, etc.) |
| `StatusCode` | Response status code |
| `ObfuscatedRequest` | Request body with sensitive data masked |
| `ObfuscatedResponse` | Response body with sensitive data masked |

Application Insights connection is configured automatically via Terraform during deployment.

## 🧪 Testing

### Run All Tests

```bash
# Run all tests
dotnet test

# Run only unit tests (fast)
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run with detailed output
dotnet test --verbosity normal
```

### Test Projects

**Unit Tests** (`tests/AzureAppServiceLoggingMiddleware.UnitTests/`)
- ObfuscationMiddleware logic testing
- Mock dependencies (ILogger, HttpContext)
- Edge cases (null bodies, invalid JSON, nested objects)
- Sensitive property detection
- Fast execution (~100ms)

**Integration Tests** (`tests/AzureAppServiceLoggingMiddleware.IntegrationTests/`)
- End-to-end API testing with WebApplicationFactory
- Real HTTP requests through middleware pipeline
- Module endpoint validation (Orders, Payments)
- Health check verification
- Slower execution (~500ms)

### Test Results

Test results are automatically published to GitHub Actions:
- ✅ Summary in workflow logs
- ✅ Detailed results in "Tests" tab
- ✅ Failure annotations on PRs
- ✅ TRX format reports

### With cURL
```bash
# Create an order
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "totalAmount": 299.99,
    "items": [{"productName": "Laptop", "quantity": 1, "price": 299.99}]
  }'

# Process payment with sensitive data
curl -X POST http://localhost:5000/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "amount": 299.99,
    "method": "CreditCard",
    "creditCard": "1234-5678-9012-3456",
    "cvv": "123"
  }'
```

### With Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Try out endpoints interactively
3. Check console logs to see obfuscated output

## 📚 Documentation

### Architecture & Design
- [Repository Structure](docs/repository-structure.md) - Folder organization
- [Module Pattern Overview](docs/module-pattern.md) - Modular architecture
- [Microservice Split Criteria](docs/microservice-split-criteria.md) - When to extract

### Infrastructure & DevOps
- [Infrastructure Guide](infrastructure/README.md) - Terraform and Azure resources
- [Setup Guide](docs/setup-guide.md) - Deploy to Azure step-by-step
- [App Service vs Functions](docs/app-service-vs-functions.md) - Service comparison
- [Pipeline Comparison](docs/pipeline-comparison.md) - CI/CD strategies

### Application
- [Application README](app/README.md) - Run and develop locally

## 🚀 CI/CD Pipeline

### 7-Stage Blue-Green Deployment (Automatic)

```
Stage 1: Build Application
   ↓
Stage 2: Provision Infrastructure (Terraform)
   ↓
Stage 3: Deploy to Green Slot
   ↓
Stage 4: Regression Tests on Green (comprehensive)
   ↓
Stage 5: Swap Green to Production
   ↓
Stage 6: Smoke Tests on Production (quick validation)
   ↓
Stage 7: Auto Rollback (if smoke tests fail)
```

**Triggers:** Push to `main` branch with changes to `app/**`, `infrastructure/**`, or `.github/workflows/**`

### Manual Rollback Workflow

On-demand rollback for post-deployment issues:

1. Go to **Actions** → **Manual Rollback**
2. Click **Run workflow**
3. Select environment: `dev`
4. Type confirmation: `ROLLBACK`
5. Review current state
6. Approve deployment (if environment protection enabled)
7. Rollback executes

**Use cases:**
- Issues discovered after successful deployment
- Performance degradation in production
- Business decision to revert changes
- Bug found by users (not caught in automated tests)

### Rollback Strategy

**Auto Rollback (Stage 7):**
- ✅ Triggers when production smoke tests fail
- ✅ Restores previous version automatically
- ✅ Verifies rollback succeeded
- ❌ Does NOT trigger for green slot test failures (production safe)

**Manual Rollback (Separate Workflow):**
- ✅ Full control over timing
- ✅ Validates current state before rollback
- ✅ Requires explicit confirmation
- ✅ Optional approval gate

### Automatic Deployment

Push to `main` branch triggers the deployment pipeline:

```bash
git add .
git commit -m "feat: new feature"
git push origin main
```

**Pipeline behavior:**
- Runs all 7 stages automatically
- Auto rollback only if production smoke tests fail
- Green slot test failures stop pipeline (production untouched)

### Pull Request Validation

Create PR to `main` triggers CI validation:

```bash
git checkout -b feature/new-feature
git add .
git commit -m "feat: add new feature"
git push origin feature/new-feature
# Create PR on GitHub
```

**CI Pipeline runs:**
- ✅ Build and test application
- ✅ Validate Terraform formatting
- ✅ Terraform plan (preview infrastructure changes)
- ✅ Comment PR with Terraform plan output

## 🔄 Migration Path

Ready for microservice extraction:

1. **Orders Service** - Extract `/Modules/Orders` to standalone service
2. **Payments Service** - Extract `/Modules/Payments` to standalone service
3. **Shared Middleware** - Keep obfuscation middleware as NuGet package

Each module has clear boundaries and can be independently deployed.

## 🛠️ Tech Stack

### Application
- **.NET 9.0** - ASP.NET Core minimal APIs
- **C# 13** - Records, pattern matching, modern features
- **Application Insights** - Azure monitoring and telemetry
- **Swagger/OpenAPI** - API documentation

### Infrastructure & DevOps
- **Terraform** - Infrastructure as Code
- **Azure App Service** - Linux-based hosting
- **GitHub Actions** - CI/CD automation
- **Bash Scripts** - Deployment utilities

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 👤 Author

**Gilbert Rios**

- GitHub: [@gilbertrios](https://github.com/gilbertrios)

## 🌟 What This Repo Demonstrates

### Infrastructure as Code (IaC)
- ✅ Terraform modules and environments
- ✅ Azure resource provisioning
- ✅ Infrastructure versioning and state management

### DevOps & CI/CD
- ✅ 7-stage automated deployment pipeline
- ✅ Blue-green deployment with dual rollback strategies
- ✅ Automated testing (regression + smoke tests)
- ✅ PR validation with Terraform plan preview
- ✅ Reusable workflows for code reuse
- ✅ Auto rollback on production failures
- ✅ Manual rollback for on-demand recovery

### Development Best Practices
- ✅ Modular architecture (Orders, Payments modules)
- ✅ Custom middleware (obfuscation)
- ✅ Clean code and SOLID principles
- ✅ Modern .NET 9.0 patterns

### Cloud & Observability
- ✅ Azure App Service deployment slots
- ✅ Application Insights integration
- ✅ Security-first approach (data obfuscation)
- ✅ Health checks and monitoring

## 🎓 Quick Links

- **[Setup Guide](docs/setup-guide.md)** - Deploy to Azure in 10 steps
- **[Project Summary](docs/project-summary.md)** - Overview and key decisions
- **[Repository Structure](docs/repository-structure.md)** - Folder organization

---

⭐ **Star this repo if you find it useful for learning or reference!**