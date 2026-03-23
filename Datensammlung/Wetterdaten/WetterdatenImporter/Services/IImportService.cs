namespace WetterdatenImporter.Services;

public interface IImportService
{
    Task<ImportRunSummary> RunDailyImportAsync(DateOnly importDate, CancellationToken cancellationToken);
}
