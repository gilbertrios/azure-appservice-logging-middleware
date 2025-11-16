# Configuration Guide

This document explains how to configure the Azure App Service Logging Middleware application.

## Overview

Configuration is managed through:
- **`appsettings.json`** - Base configuration for all environments
- **`appsettings.Development.json`** - Development-specific overrides
- **Environment Variables** - Azure App Service settings (production)
- **User Secrets** - Local development sensitive data

## Obfuscation Middleware Configuration

### Basic Configuration

Edit `app/appsettings.json`:

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

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | boolean | `true` | Enable/disable obfuscation middleware |
| `ObfuscationMask` | string | `"***REDACTED***"` | Text to replace sensitive values |
| `SensitiveProperties` | string[] | See above | List of property names to obfuscate |

### Sensitive Properties

The middleware obfuscates any JSON property whose name matches (case-insensitive) an entry in `SensitiveProperties`.

**Default properties:**
- `password` - Passwords, passphrases
- `creditCard` / `creditCardNumber` / `cardNumber` - Credit card numbers
- `cvv` - Card security codes
- `ssn` - Social Security Numbers
- `apiKey` - API keys and secrets
- `token` - Authentication tokens
- `authorization` - Authorization headers

**Add custom properties:**

```json
{
  "ObfuscationMiddleware": {
    "SensitiveProperties": [
      "password",
      "creditCard",
      "cvv",
      "accountNumber",
      "routingNumber",
      "accessToken",
      "refreshToken",
      "clientSecret"
    ]
  }
}
```

### Obfuscation Behavior

**Case-insensitive matching:**
```json
// All of these will be obfuscated:
{
  "password": "secret",
  "Password": "secret",
  "PASSWORD": "secret",
  "PaSsWoRd": "secret"
}
```

**Recursive traversal:**
```json
// Nested objects are handled:
{
  "user": {
    "name": "John",
    "credentials": {
      "password": "secret",  // ← Obfuscated
      "apiKey": "key123"     // ← Obfuscated
    }
  }
}
```

**Array handling:**
```json
// Arrays are traversed:
{
  "payments": [
    { "creditCard": "1234-5678" },  // ← Obfuscated
    { "creditCard": "9012-3456" }   // ← Obfuscated
  ]
}
```

### Custom Obfuscation Mask

Change the replacement text:

```json
{
  "ObfuscationMiddleware": {
    "ObfuscationMask": "[HIDDEN]"
  }
}
```

**Result:**
```json
{
  "creditCard": "[HIDDEN]",
  "cvv": "[HIDDEN]"
}
```

### Disable Obfuscation

For development/debugging (not recommended for production):

```json
{
  "ObfuscationMiddleware": {
    "Enabled": false
  }
}
```

## Application Insights Configuration

### Local Development

For local testing with Application Insights:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://..."
  }
}
```

**Get connection string:**
1. Go to Azure Portal
2. Navigate to Application Insights resource
3. Copy connection string from Overview page

### Production (Azure App Service)

Connection string is automatically injected by Terraform:

```hcl
# infrastructure/terraform/modules/app-service/main.tf
app_settings = {
  "ApplicationInsights__ConnectionString" = azurerm_application_insights.main.connection_string
}
```

No manual configuration needed in `appsettings.json` for deployed environments.

### Disable Application Insights

Remove or comment out the configuration:

```json
{
  // "ApplicationInsights": {
  //   "ConnectionString": ""
  // }
}
```

Application will run without telemetry.

## Logging Configuration

### Console Logging

Configure log levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "AzureAppServiceLoggingMiddleware": "Debug"
    }
  }
}
```

**Log levels:**
- `Trace` - Most verbose (all details)
- `Debug` - Debug information
- `Information` - General informational messages
- `Warning` - Warning messages
- `Error` - Error messages
- `Critical` - Critical failures
- `None` - Disable logging

### Development Overrides

Create `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "AzureAppServiceLoggingMiddleware.Middleware": "Trace"
    }
  },
  "ObfuscationMiddleware": {
    "ObfuscationMask": "[DEV-HIDDEN]"
  }
}
```

This file is:
- ✅ Used only in Development environment
- ✅ Overrides `appsettings.json` values
- ✅ Not deployed to Azure (gitignored for Production variant)

## Environment-Specific Configuration

### Development Environment

**File:** `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "ObfuscationMiddleware": {
    "Enabled": true,
    "ObfuscationMask": "***REDACTED***"
  }
}
```

### Production Environment

Configured via Azure App Service Application Settings (environment variables):

```bash
# Set via Azure CLI
az webapp config appsettings set \
  --name your-app-name \
  --resource-group your-rg \
  --settings \
    ObfuscationMiddleware__Enabled=true \
    ObfuscationMiddleware__ObfuscationMask="***CLASSIFIED***"
```

Or via Terraform:

```hcl
app_settings = {
  "ObfuscationMiddleware__Enabled" = "true"
  "ObfuscationMiddleware__ObfuscationMask" = "***CLASSIFIED***"
}
```

**Note:** Use double underscores (`__`) to represent nested JSON structure in environment variables.

## User Secrets (Local Development)

For sensitive configuration in local development:

### Initialize User Secrets

```bash
cd app
dotnet user-secrets init
```

### Add Secrets

```bash
# Add Application Insights key
dotnet user-secrets set "ApplicationInsights:ConnectionString" "InstrumentationKey=xxx..."

# Add custom settings
dotnet user-secrets set "CustomApiKey" "your-secret-key"
```

### Access in Code

```csharp
var apiKey = Configuration["CustomApiKey"];
```

**Benefits:**
- ✅ Not stored in source control
- ✅ User-specific configuration
- ✅ Overrides appsettings.json
- ✅ Only works locally (not in Azure)

## CORS Configuration

Allow cross-origin requests:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:3000",
      "https://your-frontend.com"
    ]
  }
}
```

**Configure in Program.cs:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

app.UseCors("AllowSpecificOrigins");
```

## Health Check Configuration

Health checks are enabled by default at `/health`.

### Custom Health Checks

Add custom checks:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", () => 
        database.IsConnected() 
            ? HealthCheckResult.Healthy() 
            : HealthCheckResult.Unhealthy("Database connection failed"))
    .AddCheck("api", () => 
        apiService.IsResponding() 
            ? HealthCheckResult.Healthy() 
            : HealthCheckResult.Degraded("API slow"));
```

## Swagger/OpenAPI Configuration

Swagger is enabled in Development environment by default.

### Disable Swagger

```csharp
// In Program.cs, comment out or wrap in condition:
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### Custom Swagger Settings

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Azure App Service Logging Middleware API",
        Version = "v1",
        Description = "API with automatic sensitive data obfuscation",
        Contact = new OpenApiContact
        {
            Name = "Gilbert Rios",
            Email = "gilbertrios@hotmail.com"
        }
    });
});
```

## Module Configuration

Modules are auto-discovered via reflection. No configuration needed.

**To disable a module:**

1. Remove module folder from `app/Modules/`
2. Or implement conditional registration:

```csharp
// In module's OrderModule.cs
public void RegisterServices(IServiceCollection services, IConfiguration configuration)
{
    var enableOrders = configuration.GetValue<bool>("Modules:Orders:Enabled", true);
    if (enableOrders)
    {
        services.AddScoped<IOrderService, OrderService>();
    }
}
```

**In appsettings.json:**

```json
{
  "Modules": {
    "Orders": {
      "Enabled": true
    },
    "Payments": {
      "Enabled": false
    }
  }
}
```

## Kestrel Configuration

Configure HTTP server settings:

```json
{
  "Kestrel": {
    "Limits": {
      "MaxRequestBodySize": 10485760,
      "RequestHeadersTimeout": "00:00:30"
    },
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

## Configuration Validation

### Startup Validation

Validate required configuration at startup:

```csharp
var obfuscationConfig = builder.Configuration
    .GetSection("ObfuscationMiddleware")
    .Get<ObfuscationOptions>();

if (obfuscationConfig == null)
{
    throw new InvalidOperationException(
        "ObfuscationMiddleware configuration is missing");
}
```

### Options Pattern

Use strongly-typed configuration:

```csharp
// ObfuscationOptions.cs
public class ObfuscationOptions
{
    public bool Enabled { get; set; } = true;
    public string ObfuscationMask { get; set; } = "***REDACTED***";
    public List<string> SensitiveProperties { get; set; } = new();
}

// Program.cs
builder.Services.Configure<ObfuscationOptions>(
    builder.Configuration.GetSection("ObfuscationMiddleware"));

// Usage in middleware
public ObfuscationMiddleware(
    RequestDelegate next,
    IOptions<ObfuscationOptions> options)
{
    _options = options.Value;
}
```

## Configuration Precedence

Configuration sources are applied in order (later overrides earlier):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. User Secrets (local development only)
4. Environment Variables (Azure App Service)
5. Command-line arguments

**Example:**

```json
// appsettings.json
{ "ObfuscationMask": "***REDACTED***" }

// appsettings.Development.json
{ "ObfuscationMask": "[DEV-HIDDEN]" }

// Environment Variable
ObfuscationMiddleware__ObfuscationMask=[PROD-CLASSIFIED]
```

**Result in Production:** `[PROD-CLASSIFIED]` (environment variable wins)

## Best Practices

### Security

1. ✅ **Never commit secrets to source control**
   - Use User Secrets for local development
   - Use Azure Key Vault for production secrets

2. ✅ **Enable obfuscation in all environments**
   - Protects sensitive data in logs
   - Essential for GDPR/PCI compliance

3. ✅ **Rotate sensitive properties list regularly**
   - Add new sensitive fields as application evolves
   - Review logs to find unprotected data

### Performance

1. ✅ **Keep sensitive properties list concise**
   - Only include truly sensitive fields
   - Excessive obfuscation impacts performance

2. ✅ **Use appropriate log levels**
   - Production: Information or Warning
   - Development: Debug or Trace

### Maintainability

1. ✅ **Document custom configuration**
   - Comment non-obvious settings
   - Link to documentation

2. ✅ **Use environment-specific files**
   - Keep development settings in Development file
   - Avoid environment checks in code

3. ✅ **Validate configuration on startup**
   - Fail fast if misconfigured
   - Clear error messages

## Troubleshooting

### Obfuscation Not Working

**Problem:** Sensitive data appears in logs

**Solutions:**
1. Check `ObfuscationMiddleware.Enabled` is `true`
2. Verify property name is in `SensitiveProperties` list
3. Check case sensitivity (should be case-insensitive)
4. Ensure middleware is registered in pipeline
5. Look for typos in property names

### Configuration Not Loading

**Problem:** Settings from appsettings.json not applied

**Solutions:**
1. Verify JSON syntax is valid
2. Check file is copied to output directory
3. Ensure correct environment name (Development/Production)
4. Check configuration precedence (environment variable overriding?)

### Application Insights Not Connecting

**Problem:** No telemetry data in Azure

**Solutions:**
1. Verify connection string format
2. Check network connectivity to Azure
3. Ensure Application Insights resource exists
4. Look for exceptions in application logs
5. Test with local connection string first

## Related Documentation

- [Testing Guide](testing-guide.md) - Test configuration changes
- [Setup Guide](setup-guide.md) - Initial environment setup
- [CI/CD Pipeline](cicd-pipeline.md) - Configuration in deployment
- [Repository Structure](repository-structure.md) - Configuration file locations
