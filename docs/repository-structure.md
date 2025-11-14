# Repository Structure

This document describes the organization of the repository.

## 📁 Folder Structure

```
azure-appservice-logging-middleware/
│
├── app/                                   # .NET 9.0 Application Code
│   ├── Infrastructure/                    # Core infrastructure patterns
│   ├── Middleware/                        # Obfuscation middleware
│   ├── Modules/                           # Feature modules (Orders, Payments)
│   ├── Properties/                        # Launch settings
│   │   └── launchSettings.json            # Development environment config
│   ├── Program.cs                         # Application entry point
│   └── *.csproj                           # Project file
│
├── tests/                                 # Test Projects
│   ├── AzureAppServiceLoggingMiddleware.UnitTests/
│   │   └── Middleware/
│   │       └── ObfuscationMiddlewareTests.cs
│   └── AzureAppServiceLoggingMiddleware.IntegrationTests/
│       └── ObfuscationMiddlewareIntegrationTests.cs
│
├── infrastructure/                        # Infrastructure as Code (Terraform)
│   ├── terraform/
│   │   ├── environments/
│   │   │   └── dev/                       # Dev environment config
│   │   │       ├── main.tf                # Main resources
│   │   │       ├── variables.tf           # Input variables
│   │   │       ├── outputs.tf             # Output values
│   │   │       └── terraform.tfvars       # Dev-specific values
│   │   │
│   │   └── modules/
│   │       └── app-service/               # App Service module with slots
│   │           ├── main.tf
│   │           ├── variables.tf
│   │           └── outputs.tf
│   │
│   ├── scripts/                          # Helper scripts
│   │   └── terraform-init.sh
│   │
│   └── README.md                         # Infrastructure documentation
│
├── .github/
│   └── workflows/
│       ├── deploy-blue-green.yml         # 7-stage deployment pipeline
│       ├── manual-rollback.yml           # On-demand rollback workflow
│       ├── ci-pr-validation.yml          # PR validation (build + terraform)
│       └── _build-app.yml                # Reusable build workflow
│
├── devops/                               # CI/CD Scripts & Docs
│   ├── scripts/
│   │   ├── swap-slots.sh                 # Manual slot swap
│   │   └── validate-deployment.sh        # Deployment validation
│   │
│   └── README.md                         # DevOps documentation
│
├── docs/                                 # Documentation
│   ├── app-service-vs-functions.md
│   ├── microservice-split-criteria.md
│   ├── module-pattern.md
│   ├── pipeline-comparison.md
│   ├── project-summary.md
│   ├── repository-structure.md           # This file
│   └── setup-guide.md
│
└── README.md                             # Main repository README
```

## 🎯 Folder Purpose

### `/app` - Application Code
Contains the .NET 9.0 minimal API application featuring:
- Obfuscation middleware for sensitive data
- Modular architecture (Orders, Payments modules)
- Application Insights integration
- OpenAPI/Swagger documentation

**Key Files:**
- `Program.cs` - Application startup and configuration
- `Middleware/ObfuscationMiddleware.cs` - Core obfuscation logic
- `Modules/*` - Self-contained feature modules
- `Properties/launchSettings.json` - Development environment settings

### `/tests` - Test Projects
Comprehensive test coverage with unit and integration tests:
- Unit tests for middleware logic (fast, isolated)
- Integration tests for end-to-end API testing (WebApplicationFactory)
- Test results published to GitHub Actions

**Key Files:**
- `UnitTests/Middleware/ObfuscationMiddlewareTests.cs` - Middleware unit tests
- `IntegrationTests/ObfuscationMiddlewareIntegrationTests.cs` - E2E API tests

### `/infrastructure` - Infrastructure as Code
Terraform configurations for provisioning Azure resources:
- App Service with blue-green deployment slots
- Application Insights
- Log Analytics Workspace
- Resource groups and service plans

**Key Files:**
- `terraform/environments/dev/main.tf` - Dev environment resources
- `terraform/modules/app-service/` - Reusable App Service module

### `/.github/workflows` - CI/CD Workflows
GitHub Actions workflows executed on push/PR:
- `deploy-blue-green.yml` - 7-stage deployment pipeline with auto rollback
- `manual-rollback.yml` - On-demand rollback for post-deployment issues
- `ci-pr-validation.yml` - PR validation with build, tests, and Terraform preview
- `_build-app.yml` - Reusable build workflow with test execution

### `/devops` - CI/CD Scripts & Documentation
Deployment automation scripts and DevOps documentation:
- Helper scripts for manual operations
- Deployment validation utilities
- DevOps process documentation

**Key Files:**
- `scripts/swap-slots.sh` - Manual slot swap utility
- `scripts/validate-deployment.sh` - Deployment health checks
- `README.md` - DevOps guide and pipeline documentation

### `/docs` - Documentation
Architecture decisions, patterns, and comparisons:
- Module pattern explanation
- Microservice extraction criteria
- Azure service comparisons
- Pipeline strategies

## 🔄 Deployment Flow

```
Developer Push
      ↓
GitHub Actions (.github/workflows/)
      ↓
Stage 1: Build app/ code + run tests
      ↓
Stage 2: Provision infrastructure/
      ↓
Stage 3: Deploy to green slot
      ↓
Stage 4: Run regression tests on green
      ↓
Stage 5: Swap to production
      ↓
Stage 6: Run smoke tests on production
      ↓
Stage 7: Auto rollback (if Stage 6 fails)
```

**Note:** Manual rollback workflow available for post-deployment issues.

## 🚀 Getting Started

### Run Application Locally
```bash
cd app
dotnet run
# Or press F5 in VS Code
```

### Run Tests
```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"
```

### Deploy Infrastructure
```bash
cd infrastructure/terraform/environments/dev
terraform init
terraform apply
```

### Trigger Deployment
```bash
git push origin main  # Triggers pipeline
```

## 📝 Best Practices

### Application Code (`/app`)
- ✅ Each module is self-contained
- ✅ Follow existing patterns for new modules
- ✅ Add sensitive properties to obfuscation config

### Infrastructure (`/infrastructure`)
- ✅ Always run `terraform plan` before `apply`
- ✅ Tag all resources consistently
- ✅ Document changes in module READMEs

### DevOps (`/devops`)
- ✅ Test pipeline changes in feature branches
- ✅ Update scripts when adding new validation
- ✅ Document new workflow stages

## 🔗 Related Documentation

- [Main README](../README.md) - Project overview
- [App README](../app/README.md) - Application details
- [Infrastructure README](../infrastructure/README.md) - IaC guide
- [DevOps README](../devops/README.md) - Pipeline guide
