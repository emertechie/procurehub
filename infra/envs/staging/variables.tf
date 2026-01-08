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
