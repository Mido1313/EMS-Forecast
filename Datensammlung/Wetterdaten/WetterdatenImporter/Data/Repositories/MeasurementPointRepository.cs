using Microsoft.EntityFrameworkCore;
using WetterdatenImporter.Configuration;
using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data.Repositories;

public sealed class MeasurementPointRepository : IMeasurementPointRepository
{
    private readonly AppDbContext _dbContext;

    public MeasurementPointRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SyncFromConfigurationAsync(IReadOnlyCollection<MeasurementPointConfig> points, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.MeasurementPoints
            .ToDictionaryAsync(x => x.GebietId, cancellationToken);

        var configuredGebietIds = new HashSet<int>();

        foreach (var point in points)
        {
            if (configuredGebietIds.Add(point.GebietId) == false)
            {
                throw new InvalidOperationException($"GebietId {point.GebietId} ist in der Konfiguration mehrfach vorhanden.");
            }

            if (existing.TryGetValue(point.GebietId, out var entity))
            {
                entity.Name = point.Name;
                entity.Latitude = point.Latitude;
                entity.Longitude = point.Longitude;
                entity.IsActive = true;
            }
            else
            {
                _dbContext.MeasurementPoints.Add(new MeasurementPoint
                {
                    GebietId = point.GebietId,
                    Name = point.Name,
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    IsActive = true
                });
            }
        }

        foreach (var obsolete in existing.Values.Where(x => configuredGebietIds.Contains(x.GebietId) == false))
        {
            obsolete.IsActive = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<MeasurementPoint>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return _dbContext.MeasurementPoints
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.GebietId)
            .ToListAsync(cancellationToken);
    }
}
