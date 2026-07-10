namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class WeatherRepository : GenericRepository<Weather>, IWeatherRepository
{
    public WeatherRepository(ApplicationDbContext context) : base(context) { }
}
