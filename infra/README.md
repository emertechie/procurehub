# Terraform Infrastructure

Infrastructure as Code using Terraform for ProcureHub Azure deployment.

## Prerequisites

- Terraform >= 1.0
- Azure CLI
- Azure subscription

## Bootstrap

See deployment plan in `.plans/deployment-steps.md` for detailed setup instructions.

## Structure

- `modules/` - Reusable Terraform modules
- `envs/staging/` - Staging environment configuration
- `envs/prod/` - Production environment configuration (future)
