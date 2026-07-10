namespace Persistence;

using Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IDistrictRepository,            DistrictRepository>();
        services.AddScoped<IPostalCodeRepository,          PostalCodeRepository>();
        services.AddScoped<IMunicipalityRepository,        MunicipalityRepository>();
        services.AddScoped<IPopulationRepository,          PopulationRepository>();
        services.AddScoped<IIncidentTypeRepository,        IncidentTypeRepository>();
        services.AddScoped<ILocationTypeRepository,        LocationTypeRepository>();
        services.AddScoped<IIncidentRepository,            IncidentRepository>();
        services.AddScoped<IEventRepository,               EventRepository>();
        services.AddScoped<INursingHomeRepository,         NursingHomeRepository>();
        services.AddScoped<IAttractionRepository,          AttractionRepository>();
        services.AddScoped<ITrafficHotspotRepository,      TrafficHotspotRepository>();
        services.AddScoped<IWeatherRepository,             WeatherRepository>();
        services.AddScoped<IResultRepository,              ResultRepository>();
        services.AddScoped<IPublicHolidayRepository,       PublicHolidayRepository>();
        services.AddScoped<ITrafficRepository,             TrafficRepository>();
        services.AddScoped<ITrafficAccidentRepository,     TrafficAccidentRepository>();
        services.AddScoped<ITrafficConstructionRepository, TrafficConstructionRepository>();
        services.AddScoped<IAccidentHistoryRepository,     AccidentHistoryRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
