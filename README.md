# Azure App Service with Logging Middleware & Blue/Green Deployments

## Project Overview
Enterprise-grade Azure App Service solution with:
- **Infrastructure**: Azure App Service (Dev environment) with Application Insights
- **Deployment Strategy**: Blue/Green deployment pattern
- **Security**: Custom middleware for obfuscating sensitive data in logs
- **Language**: C# (.NET)

---

## 📋 Project Outline

### 1. Infrastructure (Azure Resources)
**Objective**: Provision Azure resources for Dev environment using Infrastructure as Code

#### Core Components:
- **App Service Plan** (Dev/Test tier for cost optimization)
- **App Service** (Web App with deployment slots)
  - Production slot
  - Staging slot (for blue/green deployments)
- **Application Insights** (Logging, monitoring, and telemetry)
- **Azure Key Vault** (Secrets management)
- **Log Analytics Workspace** (Centralized logging)
- **Optional**: Storage Account (for diagnostics)

#### IaC Options to Consider:
- [ ] Bicep (Azure-native, recommended)
- [x] Terraform
- [ ] ARM Templates
- [ ] Pulumi

#### Infrastructure Questions for Later:
- Network isolation requirements (VNet integration, private endpoints)?
- Custom domain and SSL certificate needs?
- Auto-scaling rules?
- Backup and disaster recovery requirements?
- Resource naming conventions?

---

### 2. DevOps & CI/CD Pipeline
**Objective**: Automate build, test, and blue/green deployment process

#### Pipeline Stages:
1. **Build**
   - Compile C# application
   - Run unit tests
   - Code quality checks (SonarQube/etc.)
   - Build artifacts

2. **Infrastructure Deployment**
   - Validate IaC templates
   - Deploy/update Azure resources
   - Configure environment variables

3. **Application Deployment (Blue/Green)**
   - Deploy to staging slot (green)
   - **Smoke Tests** (separate stage)
     - Health checks on staging
     - Critical endpoint validation
     - Integration tests
   - Swap staging ↔ production slots
   - **Post-Swap Validation** (separate stage)
     - Production health checks
     - App Insights metrics validation
     - Response time checks
   - **Rollback** (conditional separate stage)
     - Auto-triggers on validation failure
     - Swaps slots back to previous version
     - Can be manually triggered

4. **Post-Deployment**
   - Success notifications
   - Alert configuration
   - Performance baseline updates

#### CI/CD Platform:
- [x] GitHub Actions

#### DevOps Questions for Later:
- Branch strategy (GitFlow, trunk-based)?
- Manual approval gates before production swap?
- Automated testing strategy (unit, integration, E2E)?
- Container-based deployment vs. traditional deployment?
- Multi-stage pipeline requirements?

---

### 3. Application Development (C# Middleware)
**Objective**: Create ASP.NET Core middleware to obfuscate sensitive data in logs

#### Middleware Features:
- **Request Logging**
  - Capture HTTP method, path, headers
  - Obfuscate sensitive request body fields
  - Correlation ID injection

- **Response Logging**
  - Capture status codes, headers
  - Obfuscate sensitive response body fields
  - Track response times

- **Data Obfuscation Rules**
  - Pattern-based detection (credit cards, SSN, emails, etc.)
  - JSON path-based obfuscation (specific fields)
  - Configurable sensitivity levels
  - PII detection

- **Integration with Application Insights**
  - Custom events and metrics
  - Dependency tracking
  - Exception handling
  - Performance counters

#### Project Structure:
```
/src
  /Api                          # Main Web API project
    - Program.cs
    - Startup.cs / Program.cs (minimal hosting)
  /Middleware
    - LoggingMiddleware.cs      # Core middleware
    - DataObfuscator.cs         # Obfuscation logic
    - ObfuscationRules.cs       # Configuration
  /Models
    - LogEntry.cs
    - ObfuscationConfig.cs
  /Extensions
    - ServiceCollectionExtensions.cs
/tests
  /Middleware.Tests             # Unit tests
  /Integration.Tests            # Integration tests
/infrastructure
  /terraform                    # Terraform templates
/pipelines
  /.github/workflows/           # GitHub Actions
    - deploy-bluegreen.yml
```

#### Development Questions for Later:
- .NET version (6, 7, 8, 9)?
- Minimal APIs vs. Controllers?
- Performance requirements (request throughput)?
- Specific PII/sensitive data patterns?
- Logging format preferences (JSON structured logs)?
- Request/response body size limits for logging?
- Should middleware be a separate NuGet package?

---

### 4. Configuration Management
**Objective**: Manage application and environment-specific settings

#### Configuration Sources:
- **appsettings.json** (base configuration)
- **appsettings.Development.json** (dev overrides)
- **Azure Key Vault** (secrets and connection strings)
- **App Service Configuration** (environment variables)
- **Application Insights** (instrumentation key)

#### Key Settings:
```
- Logging:ObfuscationRules
- Logging:SensitiveFields
- ApplicationInsights:ConnectionString
- FeatureFlags:EnableDetailedLogging
```

---

### 5. Monitoring & Observability
**Objective**: Comprehensive visibility into application health

#### Metrics to Track:
- Request/response times
- HTTP status code distribution
- Exception rates
- Dependency failures (DB, external APIs)
- Custom business metrics
- Deployment success/failure rates

#### Alerts:
- High error rates
- Performance degradation
- Failed deployments
- Health check failures

---

### 6. Security Considerations
- Authentication/Authorization (Azure AD, API keys)
- Data encryption (in-transit and at-rest)
- Managed Identity for Azure resource access
- CORS policies
- Rate limiting
- DDoS protection

---

### 7. Testing Strategy
- **Unit Tests**: Middleware logic, obfuscation rules
- **Integration Tests**: End-to-end request/response flows
- **Smoke Tests**: Critical path validation on staging slot
- **Post-Deployment Tests**: Production validation after swap
- **Load Tests**: Performance validation
- **Security Tests**: Verify no sensitive data in logs

---

## 🚀 Next Steps

### Phase 1: Infrastructure Setup
1. Choose IaC tool ✅ Terraform
2. Design resource architecture
3. Provision Dev environment
4. Configure Application Insights

### Phase 2: Middleware Development
1. Create ASP.NET Core project
2. Implement logging middleware
3. Build obfuscation engine
4. Add Application Insights integration

### Phase 3: DevOps Pipeline
1. Set up source control strategy ✅ GitHub
2. Create build pipeline ✅ GitHub Actions workflow created
3. Implement blue/green deployment ✅ Slot swap strategy defined
4. Add automated tests ✅ Smoke tests and validation stages included

### Phase 4: Testing & Validation
1. Test obfuscation rules
2. Validate blue/green swaps
3. Load testing
4. Security audit

---

## 📝 Decision Log

| Date | Area | Decision | Rationale |
|------|------|----------|-----------|
| 2025-11-10 | IaC | Terraform | Indicated in original repo description |
| 2025-11-10 | .NET | TBD | To be determined based on requirements |
| 2025-11-10 | CI/CD | GitHub Actions | User preference |
| 2025-11-10 | Pipeline | Separate stages for smoke tests & rollback | Better control, observability, and conditional execution |

---

## 🔍 Areas for Deep Dive

When you're ready, we can explore each area in detail:

1. **Infrastructure Design**: Resource topology, networking, scaling strategy
2. **DevOps Implementation**: ✅ Pipeline YAML created with separate stages
3. **Middleware Development**: Code structure, obfuscation algorithms, performance optimization
4. **Monitoring Setup**: Dashboard creation, alert rules, log queries
5. **Security Hardening**: Authentication flows, secrets management, compliance

---

## Pipeline Architecture (GitHub Actions)

### Workflow: `.github/workflows/deploy-bluegreen.yml`

**Stage Separation:**
```
Build → Deploy to Staging → Smoke Tests → Swap Slots → Post-Swap Validation
                                   ↓ (if fails)              ↓ (if fails)
                                   Stop                      Rollback
```

**Jobs (Stages):**
1. `build` - Compile and test application
2. `deploy-to-staging` - Deploy to staging slot
3. `smoke-tests` - **Separate stage** for staging validation
4. `swap-slots` - Swap staging→production (with environment protection)
5. `post-swap-validation` - **Separate stage** for production validation
6. `rollback` - **Conditional separate stage** (only on failure)
7. `notify-success` - Send notifications

**Key Features:**
- Each stage is a separate GitHub Actions job
- Clear job dependencies using `needs:`
- Rollback uses `if: failure()` condition
- Environment protection for manual approvals
- Placeholder for App Insights metrics checking

---

## Questions to Answer Before Implementation

### Infrastructure
- What's the expected traffic volume?
- Data residency requirements?
- Cost budget constraints?

### DevOps
- Who approves production deployments?
- What's the rollback SLA?
- How long should staging slot validation take?

### Development
- What specific data needs obfuscation (PII types)?
- Performance impact tolerance?
- Should obfuscation be reversible by authorized personnel?

---

## GitHub Secrets Required

To use the pipeline, configure these secrets in GitHub:
- `AZURE_CREDENTIALS` - Azure service principal JSON
- `AZURE_RESOURCE_GROUP` - Resource group name
- Optional: Notification webhooks (Slack, Teams, etc.)

Update these values in the workflow file:
- `AZURE_WEBAPP_NAME` - Your App Service name
- `DOTNET_VERSION` - Your .NET version
- `STAGING_SLOT_NAME` - Staging slot name (default: staging)

---

*Let me know which area you'd like to dive into first, and I'll provide detailed implementation guidance!*
- **Language**: C# (.NET)

---

## 📋 Project Outline

### 1. Infrastructure (Azure Resources)
**Objective**: Provision Azure resources for Dev environment using Infrastructure as Code

#### Core Components:
- **App Service Plan** (Dev/Test tier for cost optimization)
- **App Service** (Web App with deployment slots)
  - Production slot
  - Staging slot (for blue/green deployments)
- **Application Insights** (Logging, monitoring, and telemetry)
- **Azure Key Vault** (Secrets management)
- **Log Analytics Workspace** (Centralized logging)
- **Optional**: Storage Account (for diagnostics)

#### IaC Options to Consider:
- [ ] Bicep (Azure-native, recommended)
- [ ] Terraform
- [ ] ARM Templates
- [ ] Pulumi

#### Infrastructure Questions for Later:
- Network isolation requirements (VNet integration, private endpoints)?
- Custom domain and SSL certificate needs?
- Auto-scaling rules?
- Backup and disaster recovery requirements?
- Resource naming conventions?

---

### 2. DevOps & CI/CD Pipeline
**Objective**: Automate build, test, and blue/green deployment process

#### Pipeline Stages:
1. **Build**
   - Compile C# application
   - Run unit tests
   - Code quality checks (SonarQube/etc.)
   - Build artifacts

2. **Infrastructure Deployment**
   - Validate IaC templates
   - Deploy/update Azure resources
   - Configure environment variables

3. **Application Deployment (Blue/Green)**
   - Deploy to staging slot (green)
   - Run smoke tests on staging
   - Swap staging ↔ production slots
   - Rollback mechanism

4. **Post-Deployment**
   - Health checks
   - Alert configuration
   - Performance validation

#### CI/CD Platform Options:
- [ ] Azure DevOps (Azure Pipelines)
- [ ] GitHub Actions
- [ ] GitLab CI

#### DevOps Questions for Later:
- Branch strategy (GitFlow, trunk-based)?
- Manual approval gates before production swap?
- Automated testing strategy (unit, integration, E2E)?
- Container-based deployment vs. traditional deployment?
- Multi-stage pipeline requirements?

---

### 3. Application Development (C# Middleware)
**Objective**: Create ASP.NET Core middleware to obfuscate sensitive data in logs

#### Middleware Features:
- **Request Logging**
  - Capture HTTP method, path, headers
  - Obfuscate sensitive request body fields
  - Correlation ID injection

- **Response Logging**
  - Capture status codes, headers
  - Obfuscate sensitive response body fields
  - Track response times

- **Data Obfuscation Rules**
  - Pattern-based detection (credit cards, SSN, emails, etc.)
  - JSON path-based obfuscation (specific fields)
  - Configurable sensitivity levels
  - PII detection

- **Integration with Application Insights**
  - Custom events and metrics
  - Dependency tracking
  - Exception handling
  - Performance counters

#### Project Structure:
```
/src
  /Api                          # Main Web API project
    - Program.cs
    - Startup.cs / Program.cs (minimal hosting)
  /Middleware
    - LoggingMiddleware.cs      # Core middleware
    - DataObfuscator.cs         # Obfuscation logic
    - ObfuscationRules.cs       # Configuration
  /Models
    - LogEntry.cs
    - ObfuscationConfig.cs
  /Extensions
    - ServiceCollectionExtensions.cs
/tests
  /Middleware.Tests             # Unit tests
  /Integration.Tests            # Integration tests
/infrastructure
  /bicep or /terraform          # IaC templates
/pipelines
  - azure-pipelines.yml
  or
  - .github/workflows/
```

#### Development Questions for Later:
- .NET version (6, 7, 8, 9)?
- Minimal APIs vs. Controllers?
- Performance requirements (request throughput)?
- Specific PII/sensitive data patterns?
- Logging format preferences (JSON structured logs)?
- Request/response body size limits for logging?
- Should middleware be a separate NuGet package?

---

### 4. Configuration Management
**Objective**: Manage application and environment-specific settings

#### Configuration Sources:
- **appsettings.json** (base configuration)
- **appsettings.Development.json** (dev overrides)
- **Azure Key Vault** (secrets and connection strings)
- **App Service Configuration** (environment variables)
- **Application Insights** (instrumentation key)

#### Key Settings:
```
- Logging:ObfuscationRules
- Logging:SensitiveFields
- ApplicationInsights:ConnectionString
- FeatureFlags:EnableDetailedLogging
```

---

### 5. Monitoring & Observability
**Objective**: Comprehensive visibility into application health

#### Metrics to Track:
- Request/response times
- HTTP status code distribution
- Exception rates
- Dependency failures (DB, external APIs)
- Custom business metrics
- Deployment success/failure rates

#### Alerts:
- High error rates
- Performance degradation
- Failed deployments
- Health check failures

---

### 6. Security Considerations
- Authentication/Authorization (Azure AD, API keys)
- Data encryption (in-transit and at-rest)
- Managed Identity for Azure resource access
- CORS policies
- Rate limiting
- DDoS protection

---

### 7. Testing Strategy
- **Unit Tests**: Middleware logic, obfuscation rules
- **Integration Tests**: End-to-end request/response flows
- **Load Tests**: Performance validation
- **Security Tests**: Verify no sensitive data in logs

---

## 🚀 Next Steps

### Phase 1: Infrastructure Setup
1. Choose IaC tool (Bicep recommended)
2. Design resource architecture
3. Provision Dev environment
4. Configure Application Insights

### Phase 2: Middleware Development
1. Create ASP.NET Core project
2. Implement logging middleware
3. Build obfuscation engine
4. Add Application Insights integration

### Phase 3: DevOps Pipeline
1. Set up source control strategy
2. Create build pipeline
3. Implement blue/green deployment
4. Add automated tests

### Phase 4: Testing & Validation
1. Test obfuscation rules
2. Validate blue/green swaps
3. Load testing
4. Security audit

---

## 📝 Decision Log
Use this section to document key decisions as we progress:

| Date | Area | Decision | Rationale |
|------|------|----------|-----------|
| TBD  | IaC  | Tool selection | - |
| TBD  | .NET | Version selection | - |
| TBD  | CI/CD| Platform choice | - |

---

## 🔍 Areas for Deep Dive

When you're ready, we can explore each area in detail:

1. **Infrastructure Design**: Resource topology, networking, scaling strategy
2. **DevOps Implementation**: Pipeline YAML, deployment scripts, rollback procedures
3. **Middleware Development**: Code structure, obfuscation algorithms, performance optimization
4. **Monitoring Setup**: Dashboard creation, alert rules, log queries
5. **Security Hardening**: Authentication flows, secrets management, compliance

---

## Questions to Answer Before Implementation

### Infrastructure
- What's the expected traffic volume?
- Data residency requirements?
- Cost budget constraints?

### DevOps
- Who approves production deployments?
- What's the rollback SLA?
- How long should staging slot validation take?

### Development
- What specific data needs obfuscation (PII types)?
- Performance impact tolerance?
- Should obfuscation be reversible by authorized personnel?

---

*Let me know which area you'd like to dive into first, and I'll provide detailed implementation guidance!*
