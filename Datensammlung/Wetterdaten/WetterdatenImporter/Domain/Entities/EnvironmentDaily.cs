namespace WetterdatenImporter.Domain.Entities;

public sealed class EnvironmentDaily
{
    public int Id { get; set; }
    public int GebietId { get; set; }
    public DateOnly Date { get; set; }
    public string Source { get; set; } = string.Empty;

    public double? EuropeanAqi { get; set; }
    public double? Pm10 { get; set; }
    public double? Pm2_5 { get; set; }
    public double? NitrogenDioxide { get; set; }
    public double? Ozone { get; set; }
    public double? AlderPollen { get; set; }
    public double? BirchPollen { get; set; }
    public double? GrassPollen { get; set; }
    public double? MugwortPollen { get; set; }
    public double? RagweedPollen { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
