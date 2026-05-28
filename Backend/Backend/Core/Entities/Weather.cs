namespace Core.Entities;

using Base.Core.Entities;
using System;

using System.Collections.Generic;

public class Weather : EntityObject
{
    public required string PostalCodeId { get; set; }  
    public PostalCode? PostalCode { get; set; } = null!;

    public DateTime MeasurementDate { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Precipitation { get; set; }  
    public decimal? Snow { get; set; }
    public decimal? WindSpeed { get; set; }
    public decimal? AirQuality { get; set; }
    public decimal? PollenTotal { get; set; }
    public decimal? PollenBirch { get; set; }
    public decimal? PollenGrass { get; set; }
    public decimal? ParticulateMatter { get; set; }
    public decimal? Visibility { get; set; }
    public bool IsForecast { get; set; }

}
