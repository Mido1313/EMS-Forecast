namespace Core.Entities;

using Base.Core.Entities;
using System;   
using System.Collections.Generic;

public class AccidentHistory : EntityObject
{
    public int DistrictId { get; set; }
    public District? District { get; set; } = null!;

    public int? AreaId { get; set; }
    public int? PostalCodeCount { get; set; }
    public string? DistrictBasis { get; set; }
    public decimal? RatePer10000Weighted { get; set; }
    public int? PopulationEstimated { get; set; }
    public int? AccidentsTotalEstimated { get; set; }
    public decimal? HotspotFactor { get; set; }
    public decimal? AccidentsHotspotAdjusted { get; set; }
    public int? InjuredEstimated { get; set; }
    public int? FatalitiesEstimated { get; set; }
    public int? ReferenceYear { get; set; }
}