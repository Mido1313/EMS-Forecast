namespace Core.Entities;

using Base.Core.Entities;
using System;   

public class Population : EntityObject
{
    public int MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; } = null!;

    public int ResidentCount { get; set; }
    public string? AgeStructure { get; set; }
}
