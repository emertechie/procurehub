# Terraform Infrastructure

Infrastructure as Code using Terraform for ProcureHub Azure deployment.

## Prerequisites

- Terraform >= 1.10
- Azure CLI
- Azure subscription

## Bootstrap

Run the bootstrap script to grant required permissions to the Terraform Service Principal:

```bash
cd infra
./bootstrap.sh staging
```

This grants "Owner" role on the resource group, allowing Terraform to create role assignments.

See `.plans/deployment-steps.md` for detailed setup instructions.

## Secrets Management

### Postgres Admin Password

The postgres admin password is managed using **ephemeral resources** - the password is never stored in Terraform state.

**How it works:**

1. Key Vault module generates ephemeral password using `ephemeral "random_password"`
2. Password is written to Key Vault using write-only argument (`value_wo`)
3. Password is passed directly to postgres module as ephemeral input variable
4. Postgres uses write-only argument (`administrator_password_wo`) to set admin password
5. The password is never stored in Terraform state (only in Key Vault)

**Password stability:**

The password **does NOT change** on every `terraform apply`. The password version is tracked:

```hcl
locals {
  postgres_password_version = 1  # Increment this to rotate password
}
```

- **Normal apply (version unchanged)**: Password remains stable
- **Increment version**: New random password is generated and applied

**To rotate the password:**

1. Edit `envs/staging/main.tf`
2. Increment `postgres_password_version` (e.g., `1` → `2`)
3. Run `terraform apply`

The new password is automatically stored in Key Vault and applied to the postgres server.

## Structure

- `modules/` - Reusable Terraform modules
- `envs/staging/` - Staging environment configuration
- `envs/prod/` - Production environment configuration (future)

## Hosting Variants

The infrastructure supports two hosting variants that can be deployed independently or together:

| Variant | Resources | Use Case |
|---------|-----------|----------|
| **React** | Container App (API) + Static Web App (SPA) | React frontend with separate API |
| **Blazor** | App Service (Blazor SSR + API) | Blazor Server-Side Rendering |

Both variants share common resources: Resource Group, Key Vault, PostgreSQL, GitHub OIDC.

### Configuration

Variants are controlled by boolean variables in `envs/staging/variables.tf`:

```hcl
variable "deploy_react_variant" {
  type    = bool
  default = false
}

variable "deploy_blazor_variant" {
  type    = bool
  default = true
}
```

### Usage Examples

```bash
cd infra/envs/staging

# Deploy only Blazor variant (default)
terraform apply

# Deploy only React variant
terraform apply -var="deploy_blazor_variant=false" -var="deploy_react_variant=true"

# Deploy both variants
terraform apply -var="deploy_blazor_variant=true" -var="deploy_react_variant=true"

# Disable a variant (will destroy its resources)
terraform apply -var="deploy_react_variant=false"
```

### File Organization

```
envs/staging/
├── main.tf              # Shared resources (rg, key_vault, postgres, github_oidc)
├── react_variant.tf     # Container App + Static Web App (count-controlled)
├── blazor_variant.tf    # App Service Blazor (count-controlled)
├── variables.tf         # Input variables including deploy_* flags
└── outputs.tf           # Conditional outputs per variant
```
