namespace Core.Entities;

using Base.Core.Entities;
using System;

using System.Collections.Generic;

public class Attraction : EntityObject
{
    public required string LocationTypeName { get; set; }  

    public IList<Incident>? Incidents { get; set; } = null!;
}
