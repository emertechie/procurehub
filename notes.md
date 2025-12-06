
# Migrations

## Generating migration SQL

Usage: dotnet ef migrations script [arguments] [options]

Example:

```
dotnet ef migrations script CreateIdentitySchema -o Data/Migrations/SqlScripts/20251206_AddStaffAndDepartment.sql
```

## Updating DB to target migration

E.g. for migration of "AddStaffAndDepartment":

```
dotnet ef database update AddStaffAndDepartment
```