using Base.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Core.Entities;

public class PostalCode : EntityObject
{
    public required string Plz { get; set; } 
    public required string CityName { get; set; }

    public int DistrictId { get; set; }
    public District? District { get; set; } = null!;

    public IList<Municipality>? Municipalities { get; set; } = null!;
    public IList<Incident>? Incidents { get; set; } = null!;
    public IList<Event>? Events { get; set; } = null!;
    public IList<NursingHome>? NursingHomes { get; set; } = null!;
    public IList<Attraction>? Attractions { get; set; } = null!;
    public IList<Weather>? Weathers { get; set; } = null!;
}