using WetterdatenImporter.Configuration;
using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data.Repositories;

public interface IMeasurementPointRepository
{
    Task SyncFromConfigurationAsync(IReadOnlyCollection<MeasurementPointConfig> points, CancellationToken cancellationToken);
    Task<List<MeasurementPoint>> GetActiveAsync(CancellationToken cancellationToken);
}
