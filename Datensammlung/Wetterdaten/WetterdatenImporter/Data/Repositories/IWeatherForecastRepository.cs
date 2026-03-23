using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data.Repositories;

public interface IWeatherForecastRepository
{
    Task<SaveOutcome> AddIfNotExistsAsync(WeatherForecastDaily entity, CancellationToken cancellationToken);
}
