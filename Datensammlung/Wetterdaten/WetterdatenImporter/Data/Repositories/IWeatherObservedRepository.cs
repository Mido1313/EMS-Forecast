using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data.Repositories;

public interface IWeatherObservedRepository
{
    Task<SaveOutcome> AddIfNotExistsAsync(WeatherObservedDaily entity, CancellationToken cancellationToken);
}
