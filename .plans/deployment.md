# ProcureHub Azure Architecture + Terraform Plan (Staging & Production)

## Purpose

This document describes the target system architecture, environment layout, deployment strategy, and Terraform structure for ProcureHub. It is intended to be used as input to an LLM or human to generate Terraform files and CI/CD workflows.

Goals:

* Codify all infrastructure using Terraform
* Support staging and production environments
* Enable safe, low-downtime deployments
* Use GitHub Actions for CI/CD
* Minimise costs (proof-of-concept)
* Strongly protect the production database from accidental deletion

---

## 1. Environments & Isolation

### Environments

* **staging**: persistent pre-production environment used for testing, integration, and PR preview validation
* **production**: live environment

### Isolation model

* Single Azure subscription
* Separate resource groups per environment:

  * `rg-procurehub-staging`
  * `rg-procurehub-prod`

This provides sufficient isolation while keeping operational overhead low.

---

## 2. Azure Region

* Primary region: **North Europe** (Ireland / Dublin)
* Both staging and production use the same region

---

## 3. High-Level Architecture

Each environment contains:

* Azure Database for PostgreSQL – Flexible Server
* Azure Container Apps (ASP.NET Core API)
* Azure Static Web Apps (React frontend)
* Azure Key Vault
* Log Analytics + Application Insights

All resources are created via Terraform.

---

## 4. PostgreSQL (Flexible Server)

### Server model

* One PostgreSQL Flexible Server per environment
* Servers are never shared between environments

### Networking & access

* Public endpoint enabled
* Default firewall policy: deny all
* Access allowed via explicit IP allow-list rules
* SSL required

### Local developer access (no VPN)

* Developers connect locally using the public endpoint
* A small Azure CLI script is used to:

  * Detect the developer’s current public IP
  * Add a temporary firewall rule
  * Optionally clean up old rules

This avoids VPN complexity while keeping exposure tightly controlled.

### Backups & HA

* No zone-redundant HA (cost minimisation)
* Backups enabled with minimal retention (e.g. 7 days)

### Deletion protection (production)

Production PostgreSQL is protected using all of the following:

* Terraform `lifecycle { prevent_destroy = true }`
* Azure Resource Lock (`CanNotDelete`) on the server
* Optional additional lock on the production resource group
* Restricted RBAC (very limited Owner access)

Deleting the production DB requires multiple deliberate steps.

---

## 5. Azure Container Apps (API)

### Structure

* One Container Apps Environment per environment
* One Container App per environment

### Ingress

* Public ingress
* Azure default hostname (custom domains can be added later)

### Revisions & deployment mode

* Multiple revisions enabled
* Blue/green deployment using revision traffic splitting

### Health checks

* Startup probe
* Liveness probe
* Readiness probe

Health checks gate traffic shifting and are required for safe deployments.

### Identity & secrets

* System-assigned managed identity
* Secrets stored in Azure Key Vault
* Container App reads secrets from Key Vault at runtime

### Observability

* Logs and metrics sent to Log Analytics
* Application Insights enabled for request tracing and metrics

---

## 6. Azure Static Web Apps (Frontend)

### Instances

Two Static Web Apps are created:

* **Staging SWA**

  * Deployed from the `staging` branch
  * Persistent staging URL

* **Production SWA**

  * Deployed from the `main` branch
  * Production URL

### PR preview environments

* Pull requests targeting `main` automatically get SWA preview URLs
* All preview environments are configured to call the **staging API**

### API base URL configuration

* `VITE_API_BASE_URL` is set per environment:

  * Staging SWA → staging API URL
  * Production SWA → production API URL
  * PR previews → staging API URL

---

## 7. CI/CD Overview (GitHub Actions)

### Authentication to Azure

* Use GitHub OIDC (no stored Azure credentials)
* Azure AD application with federated identity
* RBAC scoped to the appropriate resource groups

### Container registry

* GitHub Container Registry (GHCR)
* Images are public to minimise cost
* Image tags use commit SHA (immutable)

---

## 8. API Deployment & Promotion Strategy

### Build-once, promote-many

* Docker image is built once per commit
* Same image digest is deployed to staging and production

### Staging deployment

1. Build and test API
2. Build Docker image and push to GHCR
3. Deploy image to staging Container App (100% traffic)
4. Run database migrations against staging DB
5. Run smoke tests

### Production deployment (blue/green)

1. Deploy same image to production as a new revision at **0% traffic**
2. Run database migrations against production DB
3. Smoke test the new revision directly
4. Shift traffic gradually or switch 100% after approval
5. Rollback is done by shifting traffic back to the previous revision

---

## 9. EF Core Migrations Strategy

### Key rules

* Migrations must be backward-compatible
* Avoid running migrations on application startup

### Recommended pattern

* Use expand/contract migrations
* Generate an EF Core migrations bundle
* Run migrations as a dedicated CI/CD step before traffic shifting

This ensures:

* Safe coexistence of old and new API revisions
* Predictable migration timing

---

## 10. Frontend Deployment Flow

* `staging` branch → deploy to staging SWA
* `main` branch → deploy to production SWA
* Pull requests → SWA preview environments

When no PRs exist, staging is tested via the persistent staging SWA URL.

---

## 11. Terraform State & Structure

### State

* Remote state stored in Azure Storage
* Separate state per environment

### Directory layout

```text
infra/
  modules/
    rg/
    key_vault/
    log_analytics_appinsights/
    postgres_flexible/
    container_apps_api/
    static_web_app/
    locks/
  envs/
    staging/
      main.tf
      variables.tf
      outputs.tf
      providers.tf
      backend.tf
    prod/
      main.tf
      variables.tf
      outputs.tf
      providers.tf
      backend.tf
```

---

## 12. Terraform Modules (Responsibilities)

### rg

* Resource group creation
* Common tagging

### key_vault

* Key Vault with RBAC enabled
* Secrets for DB, API config
* Managed identity access for Container Apps

### log_analytics_appinsights

* Log Analytics workspace
* Workspace-based Application Insights

### postgres_flexible

* PostgreSQL Flexible Server
* Database creation
* SSL enforcement
* Firewall baseline
* `prevent_destroy` toggle (prod only)

### container_apps_api

* Container Apps Environment
* Container App with revision support
* Ingress, probes, diagnostics
* Managed identity

### static_web_app

* Azure Static Web App per environment
* Outputs default hostname

### locks

* Resource locks for production DB (and optionally RG)

---

## 13. Required Variables (per environment)

* `env`
* `location`
* `name_prefix`
* `postgres_sku`
* `postgres_storage_mb`
* `postgres_backup_retention_days`
* `enable_prevent_destroy` (true for prod only)
* `tags`

---

## 14. Outputs

Per environment:

* API base URL
* SWA default hostname
* PostgreSQL hostname
* Key Vault name

Outputs are used by CI/CD pipelines for:

* Smoke testing
* Migration execution
* Frontend configuration

---

## 15. Operational Scripts (Non-Terraform)

### DB firewall JIT script

* Adds developer public IP to Postgres firewall
* Removes stale rules

### Smoke tests

* API health endpoint
* Optional DB connectivity check

---

## 16. Implementation Order

1. Bootstrap Terraform remote state
2. Apply resource groups
3. Deploy observability stack
4. Deploy Key Vault
5. Deploy PostgreSQL
6. Deploy Container Apps environment + API skeleton
7. Deploy Static Web Apps
8. Configure GitHub OIDC + RBAC
9. Add CI/CD workflows
10. Validate staging → production promotion

---

## 17. Explicit Design Answers

### How is a staging API version promoted to production?

By deploying the same container image digest to production as a new revision with 0% traffic, validating it, then shifting traffic.

### What is used to test staging when no PRs exist?

The persistent staging Static Web App, which always points to the staging API.

---

## 18. Deliberate Trade-offs (PoC)

* Public Postgres endpoint with strict allow-list instead of private networking
* No HA / zone redundancy
* Azure default hostnames
* Minimal SKUs for cost control

These can all be tightened later without architectural changes.
