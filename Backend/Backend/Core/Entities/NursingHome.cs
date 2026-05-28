namespace Core.Entities;

using Base.Core.Entities;
using System;

using System.Collections.Generic;

public class NursingHome : EntityObject
{
    public required string PostalCodeId { get; set; }  
    public PostalCode? PostalCode { get; set; } = null!;

    public required string Name { get; set; }
    public int? BedCount { get; set; }
}
