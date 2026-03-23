namespace WetterdatenImporter.Domain.Entities;

public sealed class MeasurementPoint
{
    public int Id { get; set; }
    public int GebietId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; } = true;
}
