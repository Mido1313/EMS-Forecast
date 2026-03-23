using Microsoft.EntityFrameworkCore;
using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data.Repositories;

public sealed class WeatherForecastRepository : IWeatherForecastRepository
{
    private readonly AppDbContext _dbContext;

    public WeatherForecastRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SaveOutcome> AddIfNotExistsAsync(WeatherForecastDaily entity, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.WeatherForecastDailies
            .AsNoTracking()
            .AnyAsync(x => x.GebietId == entity.GebietId
                           && x.ForecastDate == entity.ForecastDate
                           && x.Source == entity.Source
                           && x.ForecastRunAt == entity.ForecastRunAt,
                cancellationToken);

        if (exists)
        {
            return SaveOutcome.Skipped;
        }

        _dbContext.WeatherForecastDailies.Add(entity);

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
