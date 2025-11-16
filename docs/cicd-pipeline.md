# CI/CD Pipeline

This repository implements a production-grade CI/CD pipeline with blue-green deployment and comprehensive rollback strategies.

## Overview

The pipeline automatically builds, tests, and deploys the application to Azure App Service using GitHub Actions. It features a 7-stage deployment process with automated testing and rollback capabilities.

## 7-Stage Blue-Green Deployment

### Pipeline Stages

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

### Deployment Flow

1. **Build Application**
   - Compile .NET application
   - Run unit tests
   - Create deployment artifacts

2. **Provision Infrastructure**
   - Run Terraform to ensure infrastructure is up-to-date
   - Create or update Azure resources
   - Configure App Service settings

3. **Deploy to Green Slot**
   - Deploy application to staging slot
   - Green slot runs parallel to production
   - No impact on live traffic

4. **Regression Tests on Green**
   - Run comprehensive test suite against green slot
   - Validate all endpoints
   - Test integration with dependencies
   - Verify obfuscation middleware functionality

5. **Swap Green to Production**
   - Instant swap (zero downtime)
   - Green slot becomes production
   - Old production becomes green slot

6. **Smoke Tests on Production**
   - Quick validation of critical paths
   - Ensure application is responding
   - Check health endpoints
   - Verify basic functionality

7. **Auto Rollback**
   - Triggers if smoke tests fail
   - Swaps slots back automatically
   - Restores previous stable version
   - Logs rollback reason

## Triggers

### Automatic Deployment

The pipeline automatically triggers on:

```yaml
push:
  branches:
    - main
  paths:
    - 'app/**'
    - 'infrastructure/**'
    - '.github/workflows/**'
```

**How to deploy:**

```bash
git add .
git commit -m "feat: new feature"
git push origin main
```

The pipeline runs automatically and deploys to the dev environment.

### Pull Request Validation

CI validation runs on pull requests to `main`:

```bash
git checkout -b feature/new-feature
git add .
git commit -m "feat: add new feature"
git push origin feature/new-feature
# Create PR on GitHub
```

**CI Pipeline performs:**
- ✅ Build and test application
- ✅ Validate Terraform formatting (`terraform fmt -check`)
- ✅ Terraform plan (preview infrastructure changes)
- ✅ Comment PR with Terraform plan output
- ✅ Run unit tests
- ✅ Check code quality

**No deployment occurs** - validation only.

## Rollback Strategies

### Auto Rollback (Stage 7)

Automatically triggers when production smoke tests fail after slot swap.

**Conditions:**
- ✅ Smoke tests fail on production slot
- ✅ Critical health checks fail
- ✅ Application unresponsive

**What happens:**
1. Pipeline detects smoke test failure
2. Immediately swaps slots back
3. Previous version restored to production
4. Green slot contains failed deployment for investigation
5. Notifications sent (GitHub, logs)

**Does NOT trigger for:**
- ❌ Green slot test failures (production never touched)
- ❌ Build failures (deployment never starts)
- ❌ Infrastructure failures (deploy stage skipped)

**Benefits:**
- Zero manual intervention needed
- Fast recovery (seconds)
- Failed version preserved for debugging

### Manual Rollback Workflow

On-demand rollback for issues discovered after successful deployment.

**When to use:**
- Issues found by users (not caught in automated tests)
- Performance degradation over time
- Business decision to revert changes
- Compliance or security concerns

**How to execute:**

1. Go to **Actions** → **Manual Rollback**
2. Click **Run workflow**
3. Select parameters:
   - **Environment:** `dev` or `prod`
   - **Confirmation:** Type `ROLLBACK`
4. Review current state displayed in logs
5. Approve deployment (if environment protection enabled)
6. Rollback executes and swaps slots

**Workflow steps:**

```yaml
1. Validate current slot configuration
2. Display current production version
3. Confirm green slot has previous version
4. Swap slots (green → production)
5. Verify swap succeeded
6. Run smoke tests on restored version
7. Report rollback status
```

**Safety features:**
- Requires explicit confirmation text
- Shows current state before rollback
- Optional approval gate (for production)
- Validates slot state before swap
- Verifies rollback succeeded

## Pipeline Behavior

### Success Path

```
┌─────────────────────┐
│ Push to main        │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Build & Test        │  ← Stage 1
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Terraform Apply     │  ← Stage 2
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Deploy to Green     │  ← Stage 3
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Regression Tests    │  ← Stage 4
│ on Green Slot       │
└──────────┬──────────┘
           │
           ▼ (tests pass)
┌─────────────────────┐
│ Swap to Production  │  ← Stage 5
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Smoke Tests on Prod │  ← Stage 6
└──────────┬──────────┘
           │
           ▼ (tests pass)
┌─────────────────────┐
│ ✅ Deployment Done  │
└─────────────────────┘
```

### Failure Scenarios

#### Scenario 1: Green Slot Tests Fail

```
Build → Terraform → Deploy to Green → ❌ Tests Fail

Result: Pipeline stops
Production: Untouched (safe)
Green Slot: Contains failed deployment
Action: Fix code and push again
```

#### Scenario 2: Production Smoke Tests Fail

```
Build → Terraform → Deploy → Green Tests Pass → Swap → ❌ Smoke Tests Fail

Result: Auto Rollback (Stage 7)
Production: Swapped back to previous version
Green Slot: Contains failed deployment
Action: Investigate why smoke tests failed
```

## Workflow Files

### Main Deployment Pipeline

**File:** `.github/workflows/deploy-blue-green.yml`

Handles the full 7-stage deployment process.

**Key features:**
- Matrix strategy for multiple environments
- Conditional stages (only deploy if tests pass)
- Environment protection rules
- Artifact management
- Terraform state locking

### Manual Rollback Workflow

**File:** `.github/workflows/manual-rollback.yml`

Provides on-demand rollback capability.

**Key features:**
- Manual trigger only
- Environment selection
- Confirmation required
- State validation
- Approval gates (optional)

### PR Validation Workflow

**File:** `.github/workflows/ci-pr-validation.yml`

Validates pull requests without deploying.

**Key features:**
- Builds application
- Runs tests
- Validates Terraform
- Comments Terraform plan on PR
- Blocks merge if checks fail

### Reusable Build Workflow

**File:** `.github/workflows/_build-app.yml`

Shared build workflow called by other workflows.

**Why reusable:**
- ✅ DRY principle (Don't Repeat Yourself)
- ✅ Consistent build process
- ✅ Single place to update build logic
- ✅ Used by deployment and PR validation

## Environment Configuration

### Dev Environment

- **Auto-deploy:** Yes (on push to main)
- **Approval required:** No
- **Rollback:** Auto + Manual available
- **Terraform backend:** `backend-configs/dev.hcl`
- **Azure resources:** `rg-logmw-dev`

### Production Environment (Future)

- **Auto-deploy:** After approval
- **Approval required:** Yes (designated approvers)
- **Rollback:** Auto + Manual available
- **Terraform backend:** `backend-configs/prod.hcl`
- **Azure resources:** `rg-logmw-prod`

## Monitoring & Notifications

### Pipeline Monitoring

- **GitHub Actions UI:** View live pipeline execution
- **Status badges:** README shows build status
- **Email notifications:** On pipeline failure
- **Slack integration:** (Optional, configurable)

### Application Monitoring

- **Application Insights:** Automatic telemetry
- **Health checks:** `/health` endpoint
- **Custom metrics:** Obfuscation middleware logs
- **Alerts:** Configured via Terraform

## Best Practices Implemented

### Security
- ✅ Credentials stored in GitHub Secrets
- ✅ Terraform state in Azure Storage with encryption
- ✅ Service Principal with least privilege
- ✅ No hardcoded secrets in code

### Reliability
- ✅ Blue-green deployment (zero downtime)
- ✅ Automated rollback on failure
- ✅ Comprehensive testing before production
- ✅ Slot swap validation

### Performance
- ✅ Parallel test execution
- ✅ Artifact caching
- ✅ Terraform state locking
- ✅ Incremental deployments

### Observability
- ✅ Detailed pipeline logs
- ✅ Terraform plan preview on PRs
- ✅ Deployment history in GitHub
- ✅ Application Insights integration

## Troubleshooting

### Pipeline Fails at Build Stage

**Symptoms:** Build errors, test failures

**Solutions:**
1. Check build logs in GitHub Actions
2. Run `dotnet build` locally
3. Ensure all tests pass locally
4. Verify dependencies in `.csproj`

### Terraform Apply Fails

**Symptoms:** Infrastructure provisioning errors

**Solutions:**
1. Check Terraform logs
2. Verify Azure credentials in GitHub Secrets
3. Ensure no manual changes to Azure resources
4. Check Terraform state lock status

### Green Slot Tests Fail

**Symptoms:** Tests pass locally but fail on green slot

**Solutions:**
1. Check test logs for specific failures
2. Verify environment variables in App Service
3. Check Application Insights connection
4. Ensure database/dependencies accessible

### Smoke Tests Fail After Swap

**Symptoms:** Auto rollback triggered

**Solutions:**
1. Check what smoke test failed (logs)
2. Compare green slot vs production configuration
3. Check for environment-specific issues
4. Verify health check endpoint

### Manual Rollback Needed

**Symptoms:** Issue found after successful deployment

**Solutions:**
1. Run Manual Rollback workflow
2. Investigate issue in green slot
3. Fix and redeploy
4. Consider improving automated tests

## Next Steps

### Enhancements to Consider

1. **Multi-Environment Support**
   - Add staging environment
   - Add production environment
   - Environment-specific configurations

2. **Advanced Testing**
   - Performance testing stage
   - Security scanning (OWASP, dependency check)
   - Load testing before production swap

3. **Notifications**
   - Slack integration
   - Teams integration
   - Email notifications with details

4. **Metrics & Analytics**
   - Deployment frequency tracking
   - Mean time to recovery (MTTR)
   - Success rate monitoring
   - Rollback frequency

5. **Progressive Delivery**
   - Canary deployments (partial traffic)
   - Feature flags
   - A/B testing support

## Related Documentation

- [Setup Guide](setup-guide.md) - Initial Azure setup
- [Infrastructure Guide](../infrastructure/README.md) - Terraform details
- [Testing Guide](testing-guide.md) - Test strategy
- [Repository Structure](repository-structure.md) - Project organization
