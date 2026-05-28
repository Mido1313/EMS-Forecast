namespace Core.Entities;

using Base.Core.Entities;
using System;

using System.Collections.Generic;

public class IncidentType : EntityObject
{
    public required string IncidentTypeName { get; set; } 
    public decimal? SeverityMean { get; set; }
    public decimal? SeverityMin { get; set; }
    public decimal? SeverityMax { get; set; }

    public IList<Incident>? Incidents { get; set; } = null!;
}
