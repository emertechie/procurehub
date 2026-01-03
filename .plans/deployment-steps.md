# ProcureHub Deployment - Step-by-Step Plan

## Pre-requisites

Create a service principal. See: https://developer.hashicorp.com/terraform/tutorials/azure-get-started/azure-build#create-a-service-principal

```bash
az ad sp create-for-rbac --role="Contributor" --scopes="/subscriptions/<SUBSCRIPTION_ID>"
```

Then set the following environment variables:

```bash
ARM_CLIENT_ID=xxxx
ARM_CLIENT_SECRET=xxxx
ARM_SUBSCRIPTION_ID=xxxx
ARM_TENANT_ID=xxxx
```

## Phase 1: Staging Environment + CI/CD

### 1. Azure & Terraform Bootstrap

- [x] **1.1** Create Azure subscription (or confirm existing one)
- [x] **1.2** Install Terraform CLI locally
- [x] **1.3** Log in with `az login`
- [x] **1.4** Create resource group for Terraform state: `rg-procurehub-tfstate`
- [x] **1.5** Create storage account + container for remote state
- [x] **1.6** Create `infra/` folder structure:
  ```
  infra/
    modules/
    envs/
      staging/
  ```
- [x] **1.7** Configure `backend.tf` for staging (Azure Storage backend)
- [x] **1.8** Run `terraform init` in `infra/envs/staging/`

Commands ran:

```bash
# Create resource group for Terraform state
az group create \
  --name rg-procurehub-tfstate \
  --location northeurope

# Create storage account (name must be globally unique, lowercase, no hyphens)
az storage account create \
  --name procurehubstgtfstate \
  --resource-group rg-procurehub-tfstate \
  --location northeurope \
  --sku Standard_LRS \
  --encryption-services blob \
  --allow-blob-public-access false

# Create container for state files
az storage container create \
  --name tfstate \
  --account-name procurehubstgtfstate

cd infra/envs/staging
terraform init
```

### 2. Resource Group + Observability (Staging)

- [x] **2.1** Create `modules/rg/` module (resource group + tags)
- [x] **2.2** Create `modules/log_analytics_appinsights/` module
- [x] **2.3** Wire up both modules in `infra/envs/staging/main.tf`
- [x] **2.4** Run `terraform plan` → `terraform apply`
- [x] **2.5** Verify resources in Azure Portal

Commnds ran:

```bash
# Validate the configuration
terraform validate

# Preview what will be created
terraform plan

# Apply (creates: resource group, log analytics workspace, application insights)
terraform apply
```

### 3. Key Vault (Staging)

- [x] **3.1** Create `modules/key_vault/` module (RBAC mode enabled)
- [x] **3.2** Add to staging `main.tf`
- [x] **3.3** Apply and verify

Commands ran:

```bash
# Get your own user object ID
az ad signed-in-user show --query id -o tsv

# Assign Key Vault Administrator to yourself
az role assignment create \
  --assignee YOUR_USER_OBJECT_ID \
  --role "Key Vault Administrator" \
  --scope "/subscriptions/98a83e0e-404f-4773-9bce-c22c1e888481/resourceGroups/rg-procurehub-staging/providers/Microsoft.KeyVault/vaults/kv-procurehub-staging"
```

### 4. PostgreSQL Flexible Server (Staging)

Generate secure password in vault first, and grant SP access to secrets:

```bash
# Generate a secure password
openssl rand -base64 32

# Store it in Key Vault (replace PASSWORD with generated value)
az keyvault secret set \
  --vault-name kv-procurehub-staging \
  --name postgres-admin-password \
  --value "YOUR_GENERATED_PASSWORD"

# Get service principal object ID from client ID
SP_OBJECT_ID=$(az ad sp show --id $ARM_CLIENT_ID --query id -o tsv)

# Grant Key Vault Secrets User role
az role assignment create \
  --assignee $SP_OBJECT_ID \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/98a83e0e-404f-4773-9bce-c22c1e888481/resourceGroups/rg-procurehub-staging/providers/Microsoft.KeyVault/vaults/kv-procurehub-staging"

```

- [x] **4.1** Create `modules/postgres_flexible/` module
  - Server, database, SSL enforcement
  - Firewall rule: allow Azure services
  - No `prevent_destroy` for staging
- [x] **4.2** Add DB admin password to Key Vault (manual or via Terraform)
- [x] **4.3** Add module to staging `main.tf`
- [x] **4.4** Apply and verify

### 5. Container Apps Environment + API (Staging)

- [ ] **5.1** Create `modules/container_apps_api/` module
  - Container Apps Environment
  - Container App (placeholder image initially)
  - System-assigned managed identity
  - Key Vault access for secrets
  - Ingress + health probes
  - App Insights integration
- [ ] **5.2** Add module to staging `main.tf`
- [ ] **5.3** Configure DB connection string as env var/Key Vault reference
- [ ] **5.4** Apply (deploys with placeholder/hello-world image)
- [ ] **5.5** Verify API URL responds

### 6. Static Web App (Staging)

- [ ] **6.1** Create `modules/static_web_app/` module
- [ ] **6.2** Add to staging `main.tf`
- [ ] **6.3** Apply and note default hostname
- [ ] **6.4** Verify SWA is accessible (empty for now)

### 7. GitHub OIDC + RBAC Setup

- [ ] **7.1** Create Azure AD app registration for GitHub Actions
- [ ] **7.2** Configure federated identity credentials:
  - Subject: `repo:YOUR_ORG/ProcureHub:ref:refs/heads/staging`
  - Subject: `repo:YOUR_ORG/ProcureHub:environment:staging` (if using GH environments)
- [ ] **7.3** Assign Contributor role on `rg-procurehub-staging`
- [ ] **7.4** Store in GitHub secrets:
  - `AZURE_CLIENT_ID`
  - `AZURE_TENANT_ID`
  - `AZURE_SUBSCRIPTION_ID`
- [ ] **7.5** Test OIDC login in a minimal workflow

### 8. Dockerize the API

- [ ] **8.1** Review/update `ProcureHub.WebApi/Dockerfile`
- [ ] **8.2** Build and test locally: `docker build -t procurehub-api .`
- [ ] **8.3** Run locally with test DB to verify

### 9. CI/CD - API Build + Push

- [ ] **9.1** Create `.github/workflows/api-ci.yml`
  - Trigger: push to `staging`, PR to `main`
  - Steps: checkout, build, test, build Docker image
- [ ] **9.2** Push image to GHCR with commit SHA tag
- [ ] **9.3** Verify image appears in GitHub Packages

### 10. CI/CD - API Deploy to Staging

- [ ] **10.1** Extend workflow (or create `api-deploy-staging.yml`)
  - Authenticate via OIDC
  - Deploy image to staging Container App
  - Run EF Core migrations bundle against staging DB
- [ ] **10.2** Create EF Core migrations bundle in CI
- [ ] **10.3** Add smoke test step (health endpoint)
- [ ] **10.4** Test full flow: push to staging → deployed + migrated

### 11. CI/CD - Frontend Build + Deploy

- [ ] **11.1** Create `staging` branch in GitHub
- [ ] **11.2** Create `.github/workflows/frontend-staging.yml`
  - Build React app with `VITE_API_BASE_URL` pointing to staging API
  - Deploy to staging Static Web App
- [ ] **11.3** Configure SWA deployment token in GitHub secrets
- [ ] **11.4** Test: push to staging → frontend deployed
- [ ] **11.5** Verify staging SWA calls staging API correctly

### 12. PR Preview Environments (Optional for Phase 1)

- [ ] **12.1** Configure SWA for PR previews (auto-enabled by default)
- [ ] **12.2** Ensure PR previews use staging API URL
- [ ] **12.3** Test: open PR → preview URL works

---

## Phase 2: Production Environment (Later)

- [ ] Create `infra/envs/prod/` (copy from staging, adjust vars)
- [ ] Add `prevent_destroy` + resource locks for prod DB
- [ ] Set up OIDC federation for `main` branch / `production` environment
- [ ] Create blue/green deployment workflow for prod API
- [ ] Configure prod Static Web App for `main` branch
- [ ] Add manual approval gate for prod deploys

---

## Phase 3: Polish (Later)

- [ ] JIT firewall script for dev DB access
- [ ] Custom domains + SSL
- [ ] Tighten SKUs / scaling rules
- [ ] Alerting rules in Application Insights
- [ ] Document runbooks

---

## Quick Reference

| Resource | Staging Name Pattern |
|----------|---------------------|
| Resource Group | `rg-procurehub-staging` |
| Key Vault | `kv-procurehub-stg` |
| Log Analytics | `log-procurehub-staging` |
| App Insights | `appi-procurehub-staging` |
| PostgreSQL | `psql-procurehub-staging` |
| Container App Env | `cae-procurehub-staging` |
| Container App | `ca-procurehub-api-staging` |
| Static Web App | `swa-procurehub-staging` |

---

## Notes

- Each numbered step should be a commit (or small PR)
- Test after each `terraform apply` before moving on
- Keep staging `terraform.tfstate` in remote backend from the start
