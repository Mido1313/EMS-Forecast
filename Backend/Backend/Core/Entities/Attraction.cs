namespace Core.Entities;

using Base.Core.Entities;
using System.Collections.Generic;

public class Attraction : EntityObject
{
    public required string PostalCodeId { get; set; }
    public PostalCode? PostalCode { get; set; } = null!;

    public required string Name { get; set; }
    public decimal? RiskSummer { get; set; }
    public decimal? RiskWinter { get; set; }

    public IList<Incident>? Incidents { get; set; }
}
