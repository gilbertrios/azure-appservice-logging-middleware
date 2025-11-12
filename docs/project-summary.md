# Project Summary

**Azure App Service Logging Middleware** - A production-ready .NET 9.0 minimal API showcasing Infrastructure as Code, DevOps best practices, and secure logging with automatic data obfuscation.

## 🎯 Project Goals

This repository demonstrates:

1. **Infrastructure as Code** - Terraform for Azure resource provisioning
2. **DevOps Excellence** - 6-stage automated deployment pipeline
3. **Developer Skills** - Clean architecture, modular design, modern .NET
4. **Security Best Practices** - Automatic PII/sensitive data obfuscation
5. **Cloud-Native Patterns** - Blue-green deployments, observability, scalability

## 📊 What's Inside

### Application Features

✅ **Obfuscation Middleware** - Automatically redacts sensitive data (credit cards, passwords, tokens) from logs  
✅ **Modular Architecture** - Self-contained Orders and Payments modules  
✅ **Application Insights** - Full observability with custom telemetry  
✅ **OpenAPI/Swagger** - Complete API documentation  
✅ **Health Checks** - Kubernetes-ready health endpoints  

### Infrastructure (Terraform)

✅ **Azure App Service** - Linux-based with .NET 9.0  
✅ **Deployment Slots** - Production + Green for blue-green deployments  
✅ **Application Insights** - Integrated logging and monitoring  
✅ **Log Analytics** - Centralized log storage  
✅ **Reusable Modules** - Clean, maintainable IaC  

### DevOps Pipeline (GitHub Actions)

✅ **Stage 1: Build** - Compile, test, publish .NET application  
✅ **Stage 2: Provision** - Terraform infrastructure deployment  
✅ **Stage 3: Deploy** - Deploy to green staging slot  
✅ **Stage 4: Test** - Automated regression and smoke tests  
✅ **Stage 5: Swap** - Promote to production (zero downtime)  
✅ **Stage 6: Rollback** - Automatic rollback on failure  

## 🏗️ Architecture Highlights

### Repository Structure

```
├── app/                    # .NET 9.0 Application
├── infrastructure/         # Terraform (IaC)
├── devops/                # GitHub Actions + Scripts
└── docs/                  # Documentation
```

### Deployment Flow

```
Code Push → Build → Provision Infra → Deploy to Green → 
Run Tests → Swap to Blue → [Rollback if needed]
```

### Azure Resources (Dev Environment)

```
Resource Group: rg-logmw-dev
├── App Service Plan (Linux B1)
├── App Service (app-logmw-dev)
│   ├── Production Slot (blue)
│   └── Green Slot (staging)
├── Application Insights
└── Log Analytics Workspace
```

## 🚀 Quick Start

### Run Locally
```bash
cd app && dotnet run
```

### Deploy to Azure
```bash
# Setup Azure credentials (one time)
az ad sp create-for-rbac --name "github-actions-logmw" --role contributor --sdk-auth

# Add to GitHub Secrets as AZURE_CREDENTIALS

# Deploy
git push origin main
```

See [Setup Guide](setup-guide.md) for detailed instructions.

## 💡 Key Technical Decisions

### Why App Service (vs Functions)?
- ✅ Better for HTTP APIs with multiple endpoints
- ✅ Built-in deployment slots for blue-green
- ✅ Easier state management (in-memory caching)
- ✅ More control over middleware pipeline

### Why Terraform (vs Bicep)?
- ✅ Multi-cloud experience (Azure, AWS, GCP)
- ✅ Larger ecosystem and community
- ✅ Better for showcasing infrastructure skills
- ✅ More marketable in job interviews

### Why Blue-Green Deployment?
- ✅ Zero downtime deployments
- ✅ Instant rollback capability
- ✅ Test in production-like environment
- ✅ Industry-standard pattern

### Why Modular Architecture?
- ✅ Easy to extract to microservices later
- ✅ Clear boundaries and ownership
- ✅ Testable in isolation
- ✅ Scalable team structure

## 📈 Skills Demonstrated

### Infrastructure & DevOps
- ☑️ Terraform (infrastructure as code)
- ☑️ Azure App Service (PaaS)
- ☑️ GitHub Actions (CI/CD)
- ☑️ Blue-green deployments
- ☑️ Automated testing in pipeline
- ☑️ Infrastructure modules (reusability)
- ☑️ Bash scripting

### Development
- ☑️ .NET 9.0 / C# 13
- ☑️ Minimal APIs
- ☑️ Custom middleware
- ☑️ Dependency injection
- ☑️ Modular architecture
- ☑️ Application Insights integration
- ☑️ OpenAPI/Swagger

### Best Practices
- ☑️ Clean architecture
- ☑️ Security (data obfuscation)
- ☑️ Observability (logging, metrics)
- ☑️ Documentation
- ☑️ Git workflows
- ☑️ Automated deployments

## 🎓 Learning Outcomes

This project teaches:

1. **IaC Fundamentals** - Manage infrastructure with code
2. **CI/CD Pipelines** - Automate build, test, deploy
3. **Cloud Patterns** - Blue-green, health checks, slots
4. **Security** - Sensitive data handling
5. **Monitoring** - Application Insights integration
6. **Architecture** - Modular, scalable design

## 📊 Metrics & Monitoring

### Pipeline Metrics
- **Build Time**: ~2-3 minutes
- **Infrastructure Provisioning**: ~3-5 minutes (first), ~30s (update)
- **Deployment Time**: ~1-2 minutes
- **Total Pipeline**: ~7-11 minutes

### Application Metrics (via App Insights)
- Request rates
- Response times
- Error rates
- Custom properties (obfuscated requests)
- Dependency tracking

## 🔗 Documentation Index

### Getting Started
- [Setup Guide](setup-guide.md) - Deploy to Azure step-by-step
- [Repository Structure](repository-structure.md) - Folder organization

### Technical Guides
- [Infrastructure README](../infrastructure/README.md) - Terraform details
- [DevOps README](../devops/README.md) - Pipeline configuration
- [Application README](../app/README.md) - Local development

### Architecture Decisions
- [Module Pattern](module-pattern.md) - Modular design
- [App Service vs Functions](app-service-vs-functions.md) - Service comparison
- [Microservice Split Criteria](microservice-split-criteria.md) - When to extract
- [Pipeline Comparison](pipeline-comparison.md) - CI/CD strategies

## 🎯 Use Cases

This repository is ideal for:

- 📚 **Learning** Terraform and Azure DevOps
- 💼 **Portfolio** to showcase for job applications
- 🏢 **Enterprise** patterns for production APIs
- 🎓 **Teaching** modern DevOps practices
- 🔨 **Template** for new .NET API projects

## 🚀 Future Enhancements

Potential additions:

- [ ] Staging environment
- [ ] Production environment  
- [ ] Database integration (SQL, CosmosDB)
- [ ] Authentication (Azure AD, JWT)
- [ ] Rate limiting
- [ ] API versioning
- [ ] Load testing (k6, JMeter)
- [ ] Container support (Docker)
- [ ] Kubernetes deployment option

## 🤝 Contributing

Contributions welcome! Areas to improve:

- Additional modules (Inventory, Shipping, etc.)
- More obfuscation patterns
- Performance optimizations
- Additional tests
- Documentation improvements

## 📝 License

MIT License - see LICENSE file

## 👤 Author

**Gilbert Rios**
- GitHub: [@gilbertrios](https://github.com/gilbertrios)
- Repository: [azure-appservice-logging-middleware](https://github.com/gilbertrios/azure-appservice-logging-middleware)

---

⭐ **Star this repo** if you find it useful for learning or as a reference!
