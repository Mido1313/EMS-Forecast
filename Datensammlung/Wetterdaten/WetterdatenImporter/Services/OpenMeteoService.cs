using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WetterdatenImporter.Configuration;
using WetterdatenImporter.Domain.Entities;
using WetterdatenImporter.Services.Models;
using WetterdatenImporter.Utilities;

namespace WetterdatenImporter.Services;

public sealed class OpenMeteoService : IOpenMeteoService
{
    private readonly OpenMeteoOptions _options;
    private readonly ImportOptions _importOptions;
    private readonly IResilientHttpClient _httpClient;
    private readonly ILogger<OpenMeteoService> _logger;

    public OpenMeteoService(
        IOptions<AppOptions> appOptions,
        IResilientHttpClient httpClient,
        ILogger<OpenMeteoService> logger)
    {
        _options = appOptions.Value.Apis.OpenMeteo;
        _importOptions = appOptions.Value.Import;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ForecastDailyData?> GetForecastDailyAsync(MeasurementPoint point, DateOnly importDate, CancellationToken cancellationToken)
    {
        var timezone = string.IsNullOrWhiteSpace(_importOptions.TimeZone) ? "auto" : _importOptions.TimeZone;

        var query = BuildQuery(new Dictionary<string, string>
        {
            ["latitude"] = point.Latitude.ToString(CultureInfo.InvariantCulture),
            ["longitude"] = point.Longitude.ToString(CultureInfo.InvariantCulture),
            ["daily"] = "temperature_2m_mean,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max,relative_humidity_2m_mean,weather_code",
            ["timezone"] = timezone,
            ["start_date"] = importDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["end_date"] = importDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        });

        var requestUri = _options.ForecastPath + "?" + query;

        using var json = await _httpClient.GetJsonAsync(HttpClientNames.OpenMeteoForecast, requestUri, cancellationToken);
        if (json.RootElement.TryGetProperty("daily", out var daily) == false || daily.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning("Open-Meteo Forecast: Antwort ohne daily-Block fuer Gebiet {GebietId}.", point.GebietId);
            return null;
        }

        var dateIndex = JsonMetricReader.FindDateIndex(daily, importDate);
        if (dateIndex < 0)
        {
            _logger.LogWarning("Open-Meteo Forecast: Kein Datum {Date} fuer Gebiet {GebietId} gefunden.", importDate, point.GebietId);
            return null;
        }

        var forecastDate = JsonMetricReader.ReadDateAtIndex(daily, dateIndex) ?? importDate;

        var result = new ForecastDailyData
        {
            ForecastDate = forecastDate,
            ForecastRunAt = DateTime.SpecifyKind(importDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
            Source = _options.SourceName,
            TemperatureMean = JsonMetricReader.ReadDoubleByCandidates(daily, dateIndex, "temperature_2m_mean", "temperature_mean"),
            TemperatureMax = JsonMetricReader.ReadDoubleByCandidates(daily, dateIndex, "temperature_2m_max", "temperature_max"),
            TemperatureMin = JsonMetricReader.ReadDoubleByCandidates(daily, dateIndex, "temperature_2m_min", "temperature_min"),
            PrecipitationSum = JsonMetricReader.ReadDoubleByCandidates(daily, dateIndex, "precipitation_sum"),
            WindSpeedMax = JsonMetricReader.ReadDoubleByCandidates(daily, dateIndex, "wind_speed_10m_max", "wind_speed_max"),
            RelativeHumidityMean = JsonMetricReader.ReadDoubleByCandidates(daily, dateIndex, "relative_humidity_2m_mean", "relative_humidity_mean"),
            WeatherCode = JsonMetricReader.ReadIntByCandidates(daily, dateIndex, "weather_code")
        };

        if (HasAnyForecastValue(result) == false)
        {
            _logger.LogWarning("Open-Meteo Forecast: Keine verwertbaren Werte fuer Gebiet {GebietId}.", point.GebietId);
            return null;
        }

        return result;
    }

    public async Task<EnvironmentDailyData?> GetEnvironmentDailyAsync(MeasurementPoint point, DateOnly importDate, CancellationToken cancellationToken)
    {
        var date = importDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var timezone = string.IsNullOrWhiteSpace(_importOptions.TimeZone) ? "auto" : _importOptions.TimeZone;

        var query = BuildQuery(new Dictionary<string, string>
        {
            ["latitude"] = point.Latitude.ToString(CultureInfo.InvariantCulture),
            ["longitude"] = point.Longitude.ToString(CultureInfo.InvariantCulture),
            ["hourly"] = "european_aqi,pm10,pm2_5,nitrogen_dioxide,ozone,alder_pollen,birch_pollen,grass_pollen,mugwort_pollen,ragweed_pollen",
            ["timezone"] = timezone,
            ["start_date"] = date,
            ["end_date"] = date
        });

        var requestUri = _options.AirQualityPath + "?" + query;

        using var json = await _httpClient.GetJsonAsync(HttpClientNames.OpenMeteoAirQuality, requestUri, cancellationToken);
        if (json.RootElement.TryGetProperty("hourly", out var hourly) == false || hourly.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning("Open-Meteo Environment: Antwort ohne hourly-Block fuer Gebiet {GebietId}.", point.GebietId);
            return null;
        }

        var indices = GetIndicesForDate(hourly, importDate);
        if (indices.Count == 0)
        {
            _logger.LogWarning("Open-Meteo Environment: Keine Stundenwerte fuer Gebiet {GebietId} am {Date}.", point.GebietId, importDate);
            return null;
        }

        var result = new EnvironmentDailyData
        {
            Date = importDate,
            Source = _options.SourceName,
            EuropeanAqi = Average(hourly, "european_aqi", indices),
            Pm10 = Average(hourly, "pm10", indices),
            Pm2_5 = Average(hourly, "pm2_5", indices),
            NitrogenDioxide = Average(hourly, "nitrogen_dioxide", indices),
            Ozone = Average(hourly, "ozone", indices),
            AlderPollen = Max(hourly, "alder_pollen", indices),
            BirchPollen = Max(hourly, "birch_pollen", indices),
            GrassPollen = Max(hourly, "grass_pollen", indices),
            MugwortPollen = Max(hourly, "mugwort_pollen", indices),
            RagweedPollen = Max(hourly, "ragweed_pollen", indices)
        };

        if (HasAnyEnvironmentValue(result) == false)
        {
            _logger.LogWarning("Open-Meteo Environment: Keine verwertbaren Werte fuer Gebiet {GebietId}.", point.GebietId);
            return null;
        }

        return result;
    }

    private static string BuildQuery(IReadOnlyDictionary<string, string> queryParams)
    {
        return string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    private static List<int> GetIndicesForDate(JsonElement hourly, DateOnly date)
    {
        var indices = new List<int>();

        if (hourly.TryGetProperty("time", out var timeArray) == false || timeArray.ValueKind != JsonValueKind.Array)
        {
            return indices;
        }

        var index = 0;
        foreach (var item in timeArray.EnumerateArray())
        {
            if (TryParseDate(item, out var itemDate) && itemDate == date)
            {
                indices.Add(index);
            }

            index++;
        }

        return indices;
    }

    private static double? Average(JsonElement hourly, string propertyName, IReadOnlyCollection<int> indices)
    {
        var values = ReadValues(hourly, propertyName, indices);
        return values.Count == 0 ? null : values.Average();
    }

    private static double? Max(JsonElement hourly, string propertyName, IReadOnlyCollection<int> indices)
    {
        var values = ReadValues(hourly, propertyName, indices);
        return values.Count == 0 ? null : values.Max();
    }

    private static List<double> ReadValues(JsonElement hourly, string propertyName, IReadOnlyCollection<int> indices)
    {
        var result = new List<double>();

        if (hourly.TryGetProperty(propertyName, out var valueArray) == false || valueArray.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var maxIndex = valueArray.GetArrayLength() - 1;
        foreach (var index in indices)
        {
            if (index < 0 || index > maxIndex)
            {
                continue;
            }

            var element = valueArray[index];
            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var number))
            {
                result.Add(number);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String
                && double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                result.Add(parsed);
            }
        }

        return result;
    }

    private static bool TryParseDate(JsonElement item, out DateOnly date)
    {
        if (item.ValueKind == JsonValueKind.String)
        {
            var raw = item.GetString();

            if (DateOnly.TryParse(raw, out date))
            {
                return true;
            }

            if (DateTimeOffset.TryParse(raw, out var offsetDate))
            {
                date = DateOnly.FromDateTime(offsetDate.DateTime);
                return true;
            }

            if (DateTime.TryParse(raw, out var dateTime))
            {
                date = DateOnly.FromDateTime(dateTime);
                return true;
            }
        }

        date = default;
        return false;
    }

    private static bool HasAnyForecastValue(ForecastDailyData data)
    {
        return data.TemperatureMean.HasValue
               || data.TemperatureMax.HasValue
               || data.TemperatureMin.HasValue
               || data.PrecipitationSum.HasValue
               || data.WindSpeedMax.HasValue
               || data.RelativeHumidityMean.HasValue
               || data.WeatherCode.HasValue;
    }

    private static bool HasAnyEnvironmentValue(EnvironmentDailyData data)
    {
        return data.EuropeanAqi.HasValue
               || data.Pm10.HasValue
               || data.Pm2_5.HasValue
               || data.NitrogenDioxide.HasValue
               || data.Ozone.HasValue
               || data.AlderPollen.HasValue
               || data.BirchPollen.HasValue
               || data.GrassPollen.HasValue
               || data.MugwortPollen.HasValue
               || data.RagweedPollen.HasValue;
    }
}
