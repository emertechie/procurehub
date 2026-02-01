#!/bin/bash
set -e

# Bootstrap script for Terraform Service Principal with proper permissions
# Run this once per environment when creating infrastructure from scratch

SUBSCRIPTION_ID="${ARM_SUBSCRIPTION_ID:-98a83e0e-404f-4773-9bce-c22c1e888481}"
ENV="${1:-staging}"
LOCATION="${AZURE_LOCATION:-westeurope}"
RESOURCE_GROUP="rg-procurehub-${ENV}"
TERRAFORM_DIR="envs/$ENV"

echo "=== ProcureHub Terraform Bootstrap ==="
echo "Environment: $ENV"
echo "Location: $LOCATION"
echo "Resource Group: $RESOURCE_GROUP"
echo "Subscription: $SUBSCRIPTION_ID"
echo ""

# Check if SP credentials are set
if [ -z "$ARM_CLIENT_ID" ]; then
  echo "ERROR: ARM_CLIENT_ID not set"
  echo ""
  echo "Create a service principal first:"
  echo "  az ad sp create-for-rbac --role=\"Contributor\" --scopes=\"/subscriptions/$SUBSCRIPTION_ID\""
  echo ""
  echo "Then set these environment variables:"
  echo "  export ARM_CLIENT_ID=xxx"
  echo "  export ARM_CLIENT_SECRET=xxx"
  echo "  export ARM_SUBSCRIPTION_ID=xxx"
  echo "  export ARM_TENANT_ID=xxx"
  exit 1
fi

echo "Using Terraform SP: $ARM_CLIENT_ID"
echo ""

# Check if resource group exists
if az group show --name "$RESOURCE_GROUP" &>/dev/null; then
  echo "✓ Resource group '$RESOURCE_GROUP' exists"
else
  echo "✗ Resource group '$RESOURCE_GROUP' does not exist"
  echo ""
  echo "Options:"
  echo "  1. Run 'terraform apply' which will create it"
  echo "  2. Create it manually first:"
  echo "     az group create --name $RESOURCE_GROUP --location $LOCATION"
  echo ""
  read -p "Create resource group now? (y/n) " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Yy]$ ]]; then
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
    echo "✓ Resource group created"
  else
    echo "Skipping resource group creation"
  fi
fi

echo ""
echo "Granting 'Owner' role to Terraform SP on resource group..."
echo "This allows Terraform to create role assignments (azurerm_role_assignment resources)"

# Grant Owner role on the resource group
az role assignment create \
  --assignee "$ARM_CLIENT_ID" \
  --role "Owner" \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP" \
  2>/dev/null || echo "⚠ Role may already be assigned (this is OK)"

echo "✓ Owner role granted on resource group"
echo ""
echo "✓ Bootstrap complete!"
echo ""
echo "Notes:"
echo "  - Terraform will automatically generate and store the postgres password in Key Vault"
echo "  - The password is not stored in Terraform state (uses ephemeral resources)"
echo "  - To rotate the password, increment 'postgres_password_version' in main.tf"
echo ""
echo "You can now run:"
echo "  cd $TERRAFORM_DIR"
echo "  terraform plan"
echo "  terraform apply"

