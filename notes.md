
# Migrations

## Generating migration

Run command in the `SupportHub.WebApi` dir  

```
dotnet ef migrations add Test -p ../SupportHub
```

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

# Aspire

## Handy commands

* Run `aspire do diagnostics` to check for potential issues.
* Build one step, with debugging: `aspire do build-frontend --log-level debug`

# Notable Code Elements

## Pagination

* ToPagedResultAsync -> extension on *ordered* querable type, since you need ordered query for proper pagination
* ToPagedResultAsyncInternal -> Executing count and data query in parallel
* Ideally EF Core would be able to support window queries to run single query that returns total count

## API

* ApiPagedResponse and ApiDataResponse -> to separate API shaped responses from domain ones

## EF

* Using `.AsNoTracking()` for read only queries to remove tracking overhead