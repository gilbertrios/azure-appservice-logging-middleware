# DevOps - CI/CD Pipelines

**Note:** GitHub Actions workflows are located in `.github/workflows/` (GitHub's required location). This folder contains documentation and helper scripts.

## 🚀 Deployment Pipeline

### 7-Stage Blue-Green Deployment

```
┌─────────────────────────────────────────────────────────┐
│  Stage 1: Build Application                             │
│  • Restore dependencies                                 │
│  • Build .NET app                                       │
│  • Run unit tests                                       │
│  • Run integration tests                                │
│  • Publish test results                                 │
│  • Upload artifact                                      │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Stage 2: Provision Infrastructure                      │
│  • Terraform init                                       │
│  • Terraform plan                                       │
│  • Terraform apply (idempotent)                         │
│  • Capture outputs (app name, resource group)           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Stage 3: Deploy to Green Slot                          │
│  • Download build artifact                              │
│  • Deploy to green staging slot                         │
│  • Wait for warmup                                      │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Stage 4: Regression Tests on Green                     │
│  • Health check                                         │
│  • API endpoint tests                                   │
│  • Response time validation                             │
│  • Comprehensive functional tests                       │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Stage 5: Swap to Production (Blue)                     │
│  • Swap green → production                              │
│  • Zero-downtime deployment                             │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Stage 6: Smoke Tests on Production                     │
│  • Quick health check validation                        │
│  • Critical endpoint verification                       │
│  • Performance baseline check                           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼ (Only if Stage 6 fails)
┌─────────────────────────────────────────────────────────┐
│  Stage 7: Auto Rollback (Conditional)                   │
│  • Swap production → green (restore previous)           │
│  • Verify rollback succeeded                            │
│  • Send failure notifications                           │
└─────────────────────────────────────────────────────────┘
```

**Important:** Auto rollback only triggers if **production smoke tests (Stage 6) fail**. Green slot test failures (Stage 4) stop the pipeline without touching production.

## 📁 Structure

```
.github/workflows/                 # GitHub Actions workflows (required location)
├── deploy-blue-green.yml          # Main 7-stage deployment pipeline
├── manual-rollback.yml            # On-demand rollback workflow
├── ci-pr-validation.yml           # PR validation (build + tests + terraform)
└── _build-app.yml                 # Reusable build workflow

devops/
├── scripts/                       # Deployment helper scripts
│   ├── swap-slots.sh              # Manual slot swap
│   └── validate-deployment.sh     # Deployment validation
│
└── README.md                      # DevOps documentation (this file)
```

## 🔧 Setup

### 1. GitHub Secrets Required

Configure these secrets in your GitHub repository (`Settings → Secrets and variables → Actions`):

| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `AZURE_CREDENTIALS` | Azure Service Principal JSON | See below |

### 2. Create Azure Service Principal

```bash
# Create service principal with contributor role
az ad sp create-for-rbac \
  --name "github-actions-logmw" \
  --role contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth

# Output (save as AZURE_CREDENTIALS secret):
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "...",
  "activeDirectoryEndpointUrl": "...",
  "resourceManagerEndpointUrl": "...",
  ...
}
```

### 3. Verify Workflow Location

The workflow is at `.github/workflows/deploy-blue-green.yml` (GitHub's required location).

Commit and push if not already done:

```bash
git add .github/workflows/deploy-blue-green.yml
git commit -m "chore: add deployment workflow"
git push origin main
```

## 🎯 Triggering Deployments

### Automatic (Push to Main)

```bash
git add .
git commit -m "feat: new feature"
git push origin main  # Triggers deployment automatically
```

### Manual (Workflow Dispatch)

1. Go to GitHub Actions tab
2. Select "Deploy to Azure App Service (Blue-Green)"
3. Click "Run workflow"
4. Select environment (dev)
5. Click "Run workflow"

## 📊 Monitoring Pipeline Execution

### View Pipeline Status

```
GitHub → Actions → Deploy to Azure App Service (Blue-Green)
```

Each stage will show:
- ✅ Success (green checkmark)
- ❌ Failure (red X)
- ⏸️ Waiting for approval (if environments configured)

### Stage Execution Times (Typical)

| Stage | Duration |
|-------|----------|
| 1. Build Application (with tests) | ~3-4 minutes |
| 2. Provision Infrastructure | ~3-5 minutes (first run), ~30s (subsequent) |
| 3. Deploy to Green | ~1-2 minutes |
| 4. Regression Tests (comprehensive) | ~1-2 minutes |
| 5. Swap to Production | ~20 seconds |
| 6. Smoke Tests (quick validation) | ~30 seconds |
| 7. Auto Rollback (if triggered) | ~30 seconds |

**Total:** ~9-14 minutes for full deployment with tests

### Test Results

Test results are automatically published to GitHub Actions:
- ✅ **Tests tab** - Detailed results for each test
- ✅ **Annotations** - Failures shown on workflow summary
- ✅ **PR comments** - Test results on pull requests
- ✅ **TRX reports** - Full .NET test report format

## 🧪 Testing Locally

### Test Deployment Validation Script

```bash
# Make scripts executable
chmod +x devops/scripts/*.sh

# Test validation against local app
dotnet run --project app &
sleep 5

BASE_URL=http://localhost:5000 \
  ./devops/scripts/validate-deployment.sh

# Stop local app
kill %1
```

### Test Infrastructure

```bash
# Initialize and plan (no apply)
cd infrastructure/terraform/environments/dev
terraform init
terraform plan
```

## 🔄 Manual Operations

### Manual Rollback Workflow

For post-deployment issues not caught by automated tests:

1. Go to **GitHub Actions** → **Manual Rollback**
2. Click **Run workflow**
3. Select environment: `dev`
4. Type confirmation: `ROLLBACK`
5. Review current state validation
6. Approve deployment (if environment protection enabled)
7. Rollback executes

**Use cases:**
- Issues discovered after successful deployment
- Performance degradation in production
- Business decision to revert changes
- Bug found by end users

### Swap Slots Manually

```bash
# Set environment variables
export RESOURCE_GROUP="rg-logmw-dev"
export APP_SERVICE_NAME="app-logmw-dev"

# Execute swap
./devops/scripts/swap-slots.sh
```

### Rollback Manually

```bash
# Swap back (green → production becomes production → green)
export RESOURCE_GROUP="rg-logmw-dev"
export APP_SERVICE_NAME="app-logmw-dev"

./devops/scripts/swap-slots.sh
```

## 🛡️ GitHub Environments (Optional)

Configure GitHub environments for approval gates:

### Create Environments

```
Settings → Environments → New environment
```

Create:
1. **dev-green** - Auto-approve
2. **dev-production** - Require approval (recommended)
3. **dev-rollback** - Auto-approve

### Add Protection Rules

For `dev-production`:
- ✅ Required reviewers: [your team]
- ✅ Wait timer: 0 minutes
- ✅ Deployment branches: `main` only

## 📈 Success Criteria

### Stage 4: Regression Tests

Tests must pass on green slot:
- ✅ Health check returns HTTP 200
- ✅ `/api/orders` returns HTTP 200
- ✅ `/api/payments` returns HTTP 200
- ✅ Response time < 3 seconds
- ✅ All functional tests pass

If any test fails → **Pipeline stops, production untouched**

### Stage 6: Production Smoke Tests

Quick validation after swap to production:
- ✅ Production health check returns HTTP 200
- ✅ Critical endpoints accessible
- ✅ No immediate errors in logs

If smoke tests fail → **Stage 7 auto rollback triggers**

## 🚨 Troubleshooting

### Pipeline Fails at Stage 2 (Infrastructure)

```bash
# Check Terraform locally
cd infrastructure/terraform/environments/dev
terraform init
terraform plan

# Common issues:
# - Azure credentials not configured
# - Subscription not found
# - Resource naming conflicts
```

### Pipeline Fails at Stage 3 (Deploy)

```bash
# Check App Service exists
az webapp list --resource-group rg-logmw-dev

# Check deployment logs
az webapp log tail --name app-logmw-dev --resource-group rg-logmw-dev
```

### Pipeline Fails at Stage 4 (Tests)

```bash
# Test green slot manually
curl https://app-logmw-dev-green.azurewebsites.net/health

# Check App Service logs
az webapp log show --name app-logmw-dev --slot green --resource-group rg-logmw-dev
```

## 🎓 Learning Resources

- [GitHub Actions Docs](https://docs.github.com/actions)
- [Azure App Service Deployment](https://docs.microsoft.com/azure/app-service/deploy-github-actions)
- [Blue-Green Deployment Pattern](https://martinfowler.com/bliki/BlueGreenDeployment.html)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)

## 📝 Best Practices

✅ **Always test in green slot first**  
✅ **Run comprehensive tests before swap**  
✅ **Monitor Application Insights after swap**  
✅ **Keep rollback capability ready**  
✅ **Use environment protection rules for production**  
✅ **Tag all deployments with version/commit SHA**  
✅ **Set up alerting for deployment failures**  
✅ **Review test results in GitHub Actions UI**  
✅ **Only auto rollback on production failures (not green slot)**

## 🔗 Next Steps

1. ✅ Infrastructure deployed via Terraform
2. ✅ Application deployed to green slot
3. ✅ Unit tests implemented
4. ✅ Integration tests implemented
5. ✅ Test results published to GitHub
6. ⬜ Add performance tests
7. ⬜ Set up staging environment
8. ⬜ Set up production environment
