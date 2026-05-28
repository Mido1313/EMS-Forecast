namespace Core.Entities;

using Base.Core.Entities;
using System;   

public class Traffic : EntityObject
{
    public int SegmentId { get; set; }
    public TrafficHotspot? Segment { get; set; } = null!;

    public int DistrictId { get; set; }
    public District? District { get; set; } = null!;

    public DateTime Timestamp { get; set; }
    public decimal? AverageVehicleSpeed { get; set; }
    public decimal? TravelTime { get; set; }
    public string? TrafficStatus { get; set; }
}
