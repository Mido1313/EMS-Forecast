using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data.Repositories;

public interface IEnvironmentDailyRepository
{
    Task<SaveOutcome> AddIfNotExistsAsync(EnvironmentDaily entity, CancellationToken cancellationToken);
}
