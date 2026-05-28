namespace Core.Entities;

using Base.Core.Entities;
using System;   

public class Municipality : EntityObject
{
    public required string PostalCodeId { get; set; }  
    public PostalCode? PostalCode { get; set; } = null!;    

    public required string MunicipalityName { get; set; }

    public IList<Population>? Populations { get; set; } = null!;
}
