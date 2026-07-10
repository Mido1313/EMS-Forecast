namespace Persistence;

using Base.Persistence;
using Core.Contracts;
using Persistence.Repositories;

public class UnitOfWork : BaseUnitOfWork, IUnitOfWork
{
    public IDistrictRepository             DistrictRepository             { get; }
    public IPostalCodeRepository           PostalCodeRepository           { get; }
    public IMunicipalityRepository         MunicipalityRepository         { get; }
    public IPopulationRepository           PopulationRepository           { get; }
    public IIncidentTypeRepository         IncidentTypeRepository         { get; }
    public ILocationTypeRepository         LocationTypeRepository         { get; }
    public IIncidentRepository             IncidentRepository             { get; }
    public IEventRepository                EventRepository                { get; }
    public INursingHomeRepository          NursingHomeRepository          { get; }
    public IAttractionRepository           AttractionRepository           { get; }
    public ITrafficHotspotRepository       TrafficHotspotRepository       { get; }
    public IWeatherRepository              WeatherRepository              { get; }
    public IResultRepository               ResultRepository               { get; }
    public IPublicHolidayRepository        PublicHolidayRepository        { get; }
    public ITrafficRepository              TrafficRepository              { get; }
    public ITrafficAccidentRepository      TrafficAccidentRepository      { get; }
    public ITrafficConstructionRepository  TrafficConstructionRepository  { get; }
    public IAccidentHistoryRepository      AccidentHistoryRepository      { get; }

    public UnitOfWork(
        ApplicationDbContext           context,
        IDistrictRepository            districtRepository,
        IPostalCodeRepository          postalCodeRepository,
        IMunicipalityRepository        municipalityRepository,
        IPopulationRepository          populationRepository,
        IIncidentTypeRepository        incidentTypeRepository,
        ILocationTypeRepository        locationTypeRepository,
        IIncidentRepository            incidentRepository,
        IEventRepository               eventRepository,
        INursingHomeRepository         nursingHomeRepository,
        IAttractionRepository          attractionRepository,
        ITrafficHotspotRepository      trafficHotspotRepository,
        IWeatherRepository             weatherRepository,
        IResultRepository              resultRepository,
        IPublicHolidayRepository       publicHolidayRepository,
        ITrafficRepository             trafficRepository,
        ITrafficAccidentRepository     trafficAccidentRepository,
        ITrafficConstructionRepository trafficConstructionRepository,
        IAccidentHistoryRepository     accidentHistoryRepository
    ) : base(context)
    {
        DistrictRepository            = districtRepository;
        PostalCodeRepository          = postalCodeRepository;
        MunicipalityRepository        = municipalityRepository;
        PopulationRepository          = populationRepository;
        IncidentTypeRepository        = incidentTypeRepository;
        LocationTypeRepository        = locationTypeRepository;
        IncidentRepository            = incidentRepository;
        EventRepository               = eventRepository;
        NursingHomeRepository         = nursingHomeRepository;
        AttractionRepository          = attractionRepository;
        TrafficHotspotRepository      = trafficHotspotRepository;
        WeatherRepository             = weatherRepository;
        ResultRepository              = resultRepository;
        PublicHolidayRepository       = publicHolidayRepository;
        TrafficRepository             = trafficRepository;
        TrafficAccidentRepository     = trafficAccidentRepository;
        TrafficConstructionRepository = trafficConstructionRepository;
        AccidentHistoryRepository     = accidentHistoryRepository;
    }
}
