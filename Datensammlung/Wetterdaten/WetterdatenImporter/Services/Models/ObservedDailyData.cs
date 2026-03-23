namespace WetterdatenImporter.Services.Models;

public sealed class ObservedDailyData
{
    public DateOnly Date { get; init; }
    public string Source { get; init; } = string.Empty;

    public double? TemperatureMean { get; init; }
    public double? TemperatureMax { get; init; }
    public double? TemperatureMin { get; init; }
    public double? PrecipitationSum { get; init; }
    public double? WindSpeedMax { get; init; }
    public double? RelativeHumidityMean { get; init; }
    public int? WeatherCode { get; init; }
}
