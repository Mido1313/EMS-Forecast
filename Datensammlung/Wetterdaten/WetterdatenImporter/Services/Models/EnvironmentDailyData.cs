namespace WetterdatenImporter.Services.Models;

public sealed class EnvironmentDailyData
{
    public DateOnly Date { get; init; }
    public string Source { get; init; } = string.Empty;

    public double? EuropeanAqi { get; init; }
    public double? Pm10 { get; init; }
    public double? Pm2_5 { get; init; }
    public double? NitrogenDioxide { get; init; }
    public double? Ozone { get; init; }
    public double? AlderPollen { get; init; }
    public double? BirchPollen { get; init; }
    public double? GrassPollen { get; init; }
    public double? MugwortPollen { get; init; }
    public double? RagweedPollen { get; init; }
}
