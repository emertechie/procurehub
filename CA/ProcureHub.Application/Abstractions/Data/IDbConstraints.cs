using Microsoft.EntityFrameworkCore;

namespace ProcureHub.Application.Abstractions.Data;

public interface IDbConstraints
{
    bool IsUniqueConstraintViolation(DbUpdateException ex, string entityName, string propertyName);
    
    bool IsForeignKeyViolation(DbUpdateException ex, string entityName, string propertyName);
    
    bool IsForeignKeyViolation(DbUpdateException ex, string entityName, string relatedEntityName, string relatedEntityPropertyName);

    bool IsForeignKeyRestrictViolation(DbUpdateException ex, string entityName, string propertyName);
}
