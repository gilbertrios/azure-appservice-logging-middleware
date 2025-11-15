# Azure App Service Logging Middleware

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Azure](https://img.shields.io/badge/Azure-App%20Service-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/en-us/services/app-service/)
[![Application Insights](https://img.shields.io/badge/Application%20Insights-Enabled-00BCF2?logo=microsoft-azure)](https://azure.microsoft.com/en-us/services/monitor/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A production-ready ASP.NET Core minimal API showcasing **automatic sensitive data obfuscation** in logs with Azure Application Insights integration. Built with clean modular architecture for easy microservice extraction.

## 🎯 Key Features

- **Smart Obfuscation Middleware** - Automatically redacts sensitive data (credit cards, passwords, tokens) from logs before they reach Application Insights
- **Azure Application Insights Integration** - Seamless telemetry with custom properties and structured logging
- **Modular Architecture** - Self-contained modules (Orders, Payments) ready for microservice extraction
- **.NET 9 Minimal APIs** - Fast, lightweight, modern ASP.NET Core
- **Auto-Discovery** - Modules automatically registered via reflection
- **Configurable** - Control obfuscation patterns via `appsettings.json`
- **OpenAPI/Swagger** - Full API documentation out of the box
- **Production-Ready** - Includes health checks, structured logging, and comprehensive testing

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

## �️ Tech Stack

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

## 🚀 CI/CD Pipeline

Automated 7-stage blue-green deployment pipeline with comprehensive rollback strategies.

```
Build → Terraform → Deploy to Green → Test Green → Swap → Smoke Test → Auto Rollback (if needed)
```

**Key Features:**
- ✅ Zero-downtime deployment with blue-green slots
- ✅ Automated rollback if production smoke tests fail
- ✅ Manual rollback workflow for post-deployment issues
- ✅ PR validation with Terraform plan preview
- ✅ Comprehensive testing before production swap

**Triggers:**
- Push to `main` with changes to `app/**`, `infrastructure/**`, or `.github/workflows/**`
- Pull requests run CI validation only (no deployment)

See [CI/CD Pipeline Documentation](docs/cicd-pipeline.md) for complete details on deployment stages, rollback strategies, and troubleshooting.

## 💻 Quick Start

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

**Optional:** Customize obfuscation settings in `app/appsettings.json` - see [Configuration Guide](docs/configuration.md)

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

Customize obfuscation behavior via `app/appsettings.json`:

```json
{
  "ObfuscationMiddleware": {
    "Enabled": true,
    "ObfuscationMask": "***REDACTED***",
    "SensitiveProperties": ["password", "creditCard", "cvv", "ssn", "apiKey", "token"]
  }
}
```

**Key features:**
- Case-insensitive property matching
- Recursive JSON traversal (nested objects/arrays)
- Configurable mask pattern and sensitive property list

See [Configuration Guide](docs/configuration.md) for complete options, Application Insights setup, environment-specific settings, and user secrets.

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

Run the test suite to verify functionality:

```bash
# Run all tests
dotnet test

# Run only unit tests (fast)
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"
```

**Test Coverage:**
- Unit tests for ObfuscationMiddleware logic and edge cases
- Integration tests for full API and middleware pipeline
- Automated execution in CI/CD pipeline

See [Testing Guide](docs/testing-guide.md) for detailed test documentation, manual testing with cURL/Swagger, and coverage reports.

## 📚 Documentation

### Architecture & Design
- [Repository Structure](docs/repository-structure.md) - Folder organization
- [Module Pattern Overview](docs/module-pattern.md) - Modular architecture
- [Microservice Split Criteria](docs/microservice-split-criteria.md) - When to extract
- [MVC vs Minimal API Pipeline](docs/mvc-vs-minimal-api-pipeline.md) - Request pipeline internals

### Infrastructure & DevOps
- [Infrastructure Guide](infrastructure/README.md) - Terraform and Azure resources
- [Setup Guide](docs/setup-guide.md) - Deploy to Azure step-by-step
- [CI/CD Pipeline](docs/cicd-pipeline.md) - Deployment pipeline and rollback strategies
- [App Service vs Functions](docs/app-service-vs-functions.md) - Service comparison

### Application
- [Application README](app/README.md) - Run and develop locally
- [Testing Guide](docs/testing-guide.md) - Test strategy, commands, and coverage
- [Configuration Guide](docs/configuration.md) - Application settings and options

##  Migration Path

Ready for microservice extraction:

1. **Orders Service** - Extract `/Modules/Orders` to standalone service
2. **Payments Service** - Extract `/Modules/Payments` to standalone service
3. **Shared Middleware** - Keep obfuscation middleware as NuGet package

Each module has clear boundaries and can be independently deployed.

##  License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📞 Support & Connect

- 💼 LinkedIn: [Connect with me](https://linkedin.com/in/gilbert-rios-22586918)
- 📧 Email: gilbertrios@hotmail.com
- 💡 GitHub: [@gilbertrios](https://github.com/gilbertrios)

## 🎓 Quick Links

- **[Setup Guide](docs/setup-guide.md)** - Deploy to Azure in 10 steps
- **[Project Summary](docs/project-summary.md)** - Overview and key decisions
- **[Repository Structure](docs/repository-structure.md)** - Folder organization

---

⭐ **Star this repo if you find it useful for learning or reference!**