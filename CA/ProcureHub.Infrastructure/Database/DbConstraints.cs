using Microsoft.EntityFrameworkCore;
using ProcureHub.Application.Abstractions.Data;

namespace ProcureHub.Infrastructure.Database;

public class DbConstraints : IDbConstraints
{
    public bool IsUniqueConstraintViolation(DbUpdateException ex, string entityName, string propertyName)
    {
        var constraintName = $"IX_{entityName}_{propertyName}";
        return ex.IsUniqueConstraintViolation(constraintName);
    }

    public bool IsForeignKeyViolation(DbUpdateException ex, string entityName, string propertyName)
    {
        var constraintName = $"FK_{entityName}_{propertyName}";
        return ex.IsForeignKeyViolation(constraintName);
    }

    public bool IsForeignKeyRestrictViolation(DbUpdateException ex, string entityName, string propertyName)
    {
        var constraintName = $"FK_{entityName}_{propertyName}";
        return ex.IsForeignKeyRestrictViolation(constraintName);
    }
}
