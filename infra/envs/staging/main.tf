# Staging environment infrastructure

module "rg" {
  source   = "../../modules/rg"
  name     = "rg-${var.name_prefix}-${var.env}"
  location = var.location
  tags     = var.tags
}

# Observability module commented out for now
# module "observability" {
#   source              = "../../modules/log_analytics_appinsights"
#   name_prefix         = var.name_prefix
#   env                 = var.env
#   resource_group_name = module.rg.name
#   location            = var.location
#   tags                = var.tags
# }
