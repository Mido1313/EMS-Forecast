namespace WetterdatenImporter.Configuration;

public sealed class AppOptions
{
    public DatabaseOptions Database { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public ImportOptions Import { get; set; } = new();
    public ApiOptions Apis { get; set; } = new();
    public List<MeasurementPointConfig> MeasurementPoints { get; set; } = new();
}

public sealed class DatabaseOptions
{
    public string Path { get; set; } = "data/wetterdaten.db";
}

public sealed class LoggingOptions
{
    public string FilePath { get; set; } = "logs/import.log";
}

public sealed class ImportOptions
{
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 20;
    public string TimeZone { get; set; } = "Europe/Vienna";
    public int ObservedDataLagDays { get; set; } = 1;
    public int ObservedLookbackExtraDays { get; set; } = 2;
    public int GeosphereMinDelayMilliseconds { get; set; } = 250;
}

public sealed class ApiOptions
{
    public GeosphereOptions Geosphere { get; set; } = new();
    public OpenMeteoOptions OpenMeteo { get; set; } = new();
}

public sealed class GeosphereOptions
{
    public string BaseUrl { get; set; } = "https://dataset.api.hub.geosphere.at";
    public string DailyEndpointTemplate { get; set; } = "/v1/timeseries/historical/spartacus-v2-1d-1km?lat_lon={lat},{lon}&start={date}T00:00&end={date}T23:59&parameters=RR,TN,TX&output_format=geojson";
    public string SourceName { get; set; } = "GeoSphere Austria";
}

public sealed class OpenMeteoOptions
{
    public string ForecastBaseUrl { get; set; } = "https://api.open-meteo.com";
    public string ForecastPath { get; set; } = "/v1/forecast";

    public string AirQualityBaseUrl { get; set; } = "https://air-quality-api.open-meteo.com";
    public string AirQualityPath { get; set; } = "/v1/air-quality";

    public string SourceName { get; set; } = "Open-Meteo";
}

public sealed class MeasurementPointConfig
{
    public int GebietId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
