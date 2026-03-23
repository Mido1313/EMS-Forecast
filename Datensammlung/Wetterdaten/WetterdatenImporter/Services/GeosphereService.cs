using System.Globalization;
using Microsoft.Extensions.Options;
using WetterdatenImporter.Configuration;
using WetterdatenImporter.Domain.Entities;
using WetterdatenImporter.Services.Models;
using WetterdatenImporter.Utilities;

namespace WetterdatenImporter.Services;

public sealed class GeosphereService : IGeosphereService
{
    private static readonly SemaphoreSlim RequestGate = new(1, 1);
    private static DateTime _lastRequestUtc = DateTime.MinValue;

    private readonly GeosphereOptions _options;
    private readonly ImportOptions _importOptions;
    private readonly IResilientHttpClient _httpClient;
    private readonly ILogger<GeosphereService> _logger;

    public GeosphereService(
        IOptions<AppOptions> appOptions,
        IResilientHttpClient httpClient,
        ILogger<GeosphereService> logger)
    {
        _options = appOptions.Value.Apis.Geosphere;
        _importOptions = appOptions.Value.Import;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ObservedDailyData?> GetObservedDailyAsync(MeasurementPoint point, DateOnly date, CancellationToken cancellationToken)
    {
        await ApplyRateLimitDelayAsync(cancellationToken);
        var requestUri = BuildRequestUri(point, date);

        using var json = await _httpClient.GetJsonAsync(HttpClientNames.Geosphere, requestUri, cancellationToken);
        var container = JsonMetricReader.GetDailyContainerOrRoot(json.RootElement);

        var dateIndex = JsonMetricReader.FindDateIndex(container, date);
        if (dateIndex < 0)
        {
            _logger.LogDebug("GeoSphere: Kein expliziter Index fuer Datum {Date} in Gebiet {GebietId}. Verwende Fallback-Index 0.",
                date, point.GebietId);
            dateIndex = 0;
        }

        var temperatureMean = JsonMetricReader.ReadDoubleByCandidates(container, dateIndex,
            "temperature_mean", "temperature_2m_mean", "t_mean", "tmean", "tm");
        var temperatureMax = JsonMetricReader.ReadDoubleByCandidates(container, dateIndex,
            "temperature_max", "temperature_2m_max", "t_max", "tmax", "tx");
        var temperatureMin = JsonMetricReader.ReadDoubleByCandidates(container, dateIndex,
            "temperature_min", "temperature_2m_min", "t_min", "tmin", "tn");

        if (temperatureMean.HasValue == false && temperatureMin.HasValue && temperatureMax.HasValue)
        {
            temperatureMean = (temperatureMin.Value + temperatureMax.Value) / 2.0;
        }

        var result = new ObservedDailyData
        {
            Date = date,
            Source = _options.SourceName,
            TemperatureMean = temperatureMean,
            TemperatureMax = temperatureMax,
            TemperatureMin = temperatureMin,
            PrecipitationSum = JsonMetricReader.ReadDoubleByCandidates(container, dateIndex,
                "precipitation_sum", "rr", "precipitation", "rain_sum"),
            WindSpeedMax = JsonMetricReader.ReadDoubleByCandidates(container, dateIndex,
                "wind_speed_max", "wind_speed_10m_max", "ffx", "wind_max"),
            RelativeHumidityMean = JsonMetricReader.ReadDoubleByCandidates(container, dateIndex,
                "relative_humidity_mean", "relative_humidity_2m_mean", "relhum", "humidity_mean"),
            WeatherCode = JsonMetricReader.ReadIntByCandidates(container, dateIndex,
                "weather_code", "weathercode", "wmo_code")
        };

        if (HasAnyValue(result) == false)
        {
            _logger.LogDebug("GeoSphere lieferte keine verwertbaren Werte fuer Gebiet {GebietId} am {Date}.", point.GebietId, date);
            return null;
        }

        return result;
    }

    private string BuildRequestUri(MeasurementPoint point, DateOnly date)
    {
        var requestUri = _options.DailyEndpointTemplate
            .Replace("{lat}", point.Latitude.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{lon}", point.Longitude.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{date}", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);

        return requestUri;
    }

    private async Task ApplyRateLimitDelayAsync(CancellationToken cancellationToken)
    {
        var minDelayMs = Math.Max(0, _importOptions.GeosphereMinDelayMilliseconds);
        if (minDelayMs == 0)
        {
            return;
        }

        await RequestGate.WaitAsync(cancellationToken);
        try
        {
            var elapsedMs = (DateTime.UtcNow - _lastRequestUtc).TotalMilliseconds;
            var remainingMs = minDelayMs - elapsedMs;
            if (remainingMs > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(remainingMs), cancellationToken);
            }

            _lastRequestUtc = DateTime.UtcNow;
        }
        finally
        {
            RequestGate.Release();
        }
    }

    private static bool HasAnyValue(ObservedDailyData data)
    {
        return data.TemperatureMean.HasValue
               || data.TemperatureMax.HasValue
               || data.TemperatureMin.HasValue
               || data.PrecipitationSum.HasValue
               || data.WindSpeedMax.HasValue
               || data.RelativeHumidityMean.HasValue
               || data.WeatherCode.HasValue;
    }
}
