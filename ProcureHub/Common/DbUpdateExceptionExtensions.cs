using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ProcureHub.Common;

public static class DbUpdateExceptionExtensions
{
    extension(DbUpdateException ex)
    {
        public bool IsUniqueConstraintViolation(string constraintName)
        {
            return ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == "23505" &&
                pgEx.ConstraintName == constraintName;
        }

        public bool IsForeignKeyViolation(string constraintName)
        {
            return ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == "23503" &&
                pgEx.ConstraintName == constraintName;
        }

        public bool IsForeignKeyRestrictViolation(string constraintName)
        {
            return ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == "23001" &&
                pgEx.ConstraintName == constraintName;
        }
    }
}
