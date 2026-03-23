using Microsoft.EntityFrameworkCore;

namespace WetterdatenImporter.Data.Repositories;

internal static class RepositorySaveHelpers
{
    public static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true;
    }
}
