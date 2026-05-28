namespace Core.Entities;

using Base.Core.Entities;
using System;
using System.Collections.Generic;

public class TrafficHotspot : EntityObject
{

    public required string LinkId { get; set; }
    public required string HotspotName { get; set; }
    public string? RoadType { get; set; }
    public decimal? CriticalityWeight { get; set; }
    public decimal? FreeFlowSpeedKph { get; set; }
    public decimal? LengthKm { get; set; }
    public bool? IsTouristic { get; set; }
    public bool? IsCommuter { get; set; }

    public int DistrictId { get; set; }
    public District? District { get; set; } = null!;

    public IList<Traffic>? Traffics { get; set; } = null!;
    public IList<TrafficAccident>? TrafficAccidents { get; set; } = null!;
    public IList<TrafficConstruction>? TrafficConstructions { get; set; } = null!;

}