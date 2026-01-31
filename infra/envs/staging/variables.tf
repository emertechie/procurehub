variable "env" {
  description = "Environment name"
  type        = string
  default     = "staging"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "northeurope"
}

variable "location2" {
  description = "Azure region for services not available in primary location (e.g. Static Web Apps)"
  type        = string
  default     = "westeurope"
}

variable "name_prefix" {
  description = "Prefix for resource names"
  type        = string
  default     = "procurehub"
}

variable "github_repo" {
  description = "GitHub repository in format 'owner/repo'"
  type        = string
  default     = "emertechie/procurehub"
}

variable "tags" {
  description = "Common tags for all resources"
  type        = map(string)
  default = {
    Project     = "ProcureHub"
    Environment = "staging"
    ManagedBy   = "Terraform"
  }
}

# Hosting variant flags - can both be true to deploy both variants
variable "deploy_react_variant" {
  description = "Deploy React variant: Container App API + Static Web App"
  type        = bool
  default     = false
}

variable "deploy_blazor_variant" {
  description = "Deploy Blazor variant: App Service (Blazor SSR + API)"
  type        = bool
  default     = false
}
