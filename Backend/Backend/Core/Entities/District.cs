using Base.Core.Entities;

namespace Core.Entities;

using System.Collections.Generic;

public class District : EntityObject
{
    public required int DistrictId { get; set; }

    public required string DistrictName { get; set; }

    public IList<PostalCode>? PostalCodes { get; set; } = null!;
    public IList<TrafficHotspot>? TrafficHotspots { get; set; } = null!;
    public IList<Traffic>? Traffics { get; set; } = null!;
    public IList<TrafficAccident>? TrafficAccidents { get; set; } = null!;
    public IList<TrafficConstruction>? TrafficConstructions { get; set; } = null!;
    public IList<AccidentHistory>? AccidentHistories { get; set; } = null!;
    public IList<Result>? Results { get; set; } = null!;
}