using Microsoft.EntityFrameworkCore;
using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data.Repositories;

public sealed class WeatherObservedRepository : IWeatherObservedRepository
{
    private readonly AppDbContext _dbContext;

    public WeatherObservedRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SaveOutcome> AddIfNotExistsAsync(WeatherObservedDaily entity, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.WeatherObservedDailies
            .AsNoTracking()
            .AnyAsync(x => x.GebietId == entity.GebietId && x.Date == entity.Date && x.Source == entity.Source, cancellationToken);

        if (exists)
        {
            return SaveOutcome.Skipped;
        }

        _dbContext.WeatherObservedDailies.Add(entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return SaveOutcome.Saved;
        }
        catch (DbUpdateException ex) when (RepositorySaveHelpers.IsUniqueConstraintViolation(ex))
        {
            _dbContext.Entry(entity).State = EntityState.Detached;
            return SaveOutcome.Skipped;
        }
    }
}
