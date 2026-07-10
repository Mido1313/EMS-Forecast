namespace Core.Contracts;

using Base.Core.Contracts;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IDistrictRepository DistrictRepository { get; }
    IPostalCodeRepository PostalCodeRepository { get; }
    IMunicipalityRepository MunicipalityRepository { get; }
    IPopulationRepository PopulationRepository { get; }
    IIncidentTypeRepository IncidentTypeRepository { get; }
    ILocationTypeRepository LocationTypeRepository { get; }
    IIncidentRepository IncidentRepository { get; }
    IEventRepository EventRepository { get; }
    INursingHomeRepository NursingHomeRepository { get; }
    IAttractionRepository AttractionRepository { get; }
    ITrafficHotspotRepository TrafficHotspotRepository { get; }
    IWeatherRepository WeatherRepository { get; }
    IResultRepository ResultRepository { get; }
    IPublicHolidayRepository PublicHolidayRepository { get; }
    ITrafficRepository TrafficRepository { get; }
    ITrafficAccidentRepository TrafficAccidentRepository { get; }
    ITrafficConstructionRepository TrafficConstructionRepository { get; }
    IAccidentHistoryRepository AccidentHistoryRepository { get; }
}
