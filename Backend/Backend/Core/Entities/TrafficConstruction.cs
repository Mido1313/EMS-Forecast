namespace Core.Entities;

using Base.Core.Entities;
using System;
using System.Collections.Generic;

public class TrafficConstruction : EntityObject
{
    public int DistrictId { get; set; }
    public District? District { get; set; } = null!;

    public int SegmentId { get; set; }
    public TrafficHotspot? Segment { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Severity { get; set; }
}