using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ProcureHub.Infrastructure.Database;

public static class DbUpdateExceptionExtensions
{
    // SQL Server error numbers
    private const int UniqueConstraintViolationError = 2627;  // Violation of UNIQUE KEY constraint
    private const int UniqueIndexViolationError = 2601;       // Cannot insert duplicate key row
    private const int ForeignKeyViolationError = 547;         // FK constraint violation (insert/update/delete)

    extension(DbUpdateException ex)
    {
        public bool IsUniqueConstraintViolation(string constraintName)
        {
            return ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == UniqueConstraintViolationError || sqlEx.Number == UniqueIndexViolationError) &&
                sqlEx.Message.Contains(constraintName, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsForeignKeyViolation(string constraintName)
        {
            return ex.InnerException is SqlException sqlEx &&
                sqlEx.Number == ForeignKeyViolationError &&
                sqlEx.Message.Contains(constraintName, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsForeignKeyRestrictViolation(string constraintName)
        {
            // SQL Server uses the same error number (547) for all FK violations
            // The difference between RESTRICT and other actions is in the constraint definition,
            // not the error handling
            return ex.IsForeignKeyViolation(constraintName);
        }
    }
}
