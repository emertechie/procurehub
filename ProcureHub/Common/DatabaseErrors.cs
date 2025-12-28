using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ProcureHub.Common;

public static class DatabaseErrors
{
    public static bool IsUniqueConstraintViolation(DbUpdateException ex, string constraintName)
    {
        return ex.InnerException is PostgresException pgEx &&
               pgEx.ConstraintName == constraintName;
    }
}
