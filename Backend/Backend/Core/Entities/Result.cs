namespace Core.Entities;

using Base.Core.Entities;
using System;

public class Result : EntityObject
{
    public int DistrictId { get; set; }
    public District? District { get; set; } = null!;

    public DateTime WindowFrom { get; set; }
    public DateTime WindowTo { get; set; }
    public decimal? RiskScore { get; set; }
    public string? RiskLevel { get; set; }
    public decimal? ScoreTraffic { get; set; }
    public decimal? ScoreAccident { get; set; }
    public decimal? ScoreWeather { get; set; }
    public decimal? ScoreHoliday { get; set; }
    public decimal? ScoreEvent { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModelVersion { get; set; }
    public decimal? Confidence { get; set; }



}
