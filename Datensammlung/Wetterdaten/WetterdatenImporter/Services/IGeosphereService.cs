using WetterdatenImporter.Domain.Entities;
using WetterdatenImporter.Services.Models;

namespace WetterdatenImporter.Services;

public interface IGeosphereService
{
    Task<ObservedDailyData?> GetObservedDailyAsync(MeasurementPoint point, DateOnly date, CancellationToken cancellationToken);
}
