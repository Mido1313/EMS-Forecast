namespace Core.Entities;

using Base.Core.Entities;
using System;

using System.Collections.Generic;

public class Incident : EntityObject
{
    public required string PostalCodeId { get; set; }  
    public PostalCode? PostalCode { get; set; } = null!;

    public int LocationTypeId { get; set; }
    public LocationType? LocationType { get; set; } = null!;

    public int IncidentTypeId { get; set; }
    public IncidentType? IncidentType { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public int? Age { get; set; }
}
