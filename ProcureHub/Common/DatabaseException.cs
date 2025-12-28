using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ProcureHub.Common;

public static class DatabaseException
{
    public static bool IsUniqueConstraintViolation(DbUpdateException ex, string constraintName)
    {
        return ex.InnerException is PostgresException pgEx &&
            pgEx.SqlState == "23505" &&
            pgEx.ConstraintName == constraintName;
    }
}
