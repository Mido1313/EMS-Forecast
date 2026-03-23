namespace WetterdatenImporter.Domain.Entities;

public sealed class WeatherForecastDaily
{
    public int Id { get; set; }
    public int GebietId { get; set; }
    public DateOnly ForecastDate { get; set; }
    public DateTime ForecastRunAt { get; set; }
    public string Source { get; set; } = string.Empty;

    public double? TemperatureMean { get; set; }
    public double? TemperatureMax { get; set; }
    public double? TemperatureMin { get; set; }
    public double? PrecipitationSum { get; set; }
    public double? WindSpeedMax { get; set; }
    public double? RelativeHumidityMean { get; set; }
    public int? WeatherCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
