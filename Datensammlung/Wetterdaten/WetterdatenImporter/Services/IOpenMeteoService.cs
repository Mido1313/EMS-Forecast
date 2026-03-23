using WetterdatenImporter.Domain.Entities;
using WetterdatenImporter.Services.Models;

namespace WetterdatenImporter.Services;

public interface IOpenMeteoService
{
    Task<ForecastDailyData?> GetForecastDailyAsync(MeasurementPoint point, DateOnly importDate, CancellationToken cancellationToken);
    Task<EnvironmentDailyData?> GetEnvironmentDailyAsync(MeasurementPoint point, DateOnly importDate, CancellationToken cancellationToken);
}
