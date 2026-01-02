terraform {
  backend "azurerm" {
    resource_group_name  = "rg-procurehub-tfstate"
    storage_account_name = "procurehubstgtfstate"
    container_name       = "tfstate"
    key                  = "staging.tfstate"
  }
}
