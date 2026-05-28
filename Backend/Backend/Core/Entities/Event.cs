namespace Core.Entities;

using Base.Core.Entities;
using System;

using System.Collections.Generic;

public class Event : EntityObject
{
    public required string PostalCodeId { get; set; } 
    public PostalCode? PostalCode { get; set; } = null!;

    public required string Name { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int? ExpectedVisitors { get; set; }
}
