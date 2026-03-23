using Microsoft.Extensions.Options;
using WetterdatenImporter.Configuration;
using WetterdatenImporter.Data.Repositories;
using WetterdatenImporter.Domain.Entities;
using WetterdatenImporter.Services.Models;

namespace WetterdatenImporter.Services;

public sealed class ImportService : IImportService
{
    private readonly AppOptions _appOptions;
    private readonly IMeasurementPointRepository _measurementPointRepository;
    private readonly IWeatherObservedRepository _weatherObservedRepository;
    private readonly IWeatherForecastRepository _weatherForecastRepository;
    private readonly IEnvironmentDailyRepository _environmentDailyRepository;
    private readonly IGeosphereService _geosphereService;
    private readonly IOpenMeteoService _openMeteoService;
    private readonly ILogger<ImportService> _logger;

    public ImportService(
        IOptions<AppOptions> appOptions,
        IMeasurementPointRepository measurementPointRepository,
        IWeatherObservedRepository weatherObservedRepository,
        IWeatherForecastRepository weatherForecastRepository,
        IEnvironmentDailyRepository environmentDailyRepository,
        IGeosphereService geosphereService,
        IOpenMeteoService openMeteoService,
        ILogger<ImportService> logger)
    {
        _appOptions = appOptions.Value;
        _measurementPointRepository = measurementPointRepository;
        _weatherObservedRepository = weatherObservedRepository;
        _weatherForecastRepository = weatherForecastRepository;
        _environmentDailyRepository = environmentDailyRepository;
        _geosphereService = geosphereService;
        _openMeteoService = openMeteoService;
        _logger = logger;
    }

    public async Task<ImportRunSummary> RunDailyImportAsync(DateOnly importDate, CancellationToken cancellationToken)
    {
        var summary = new ImportRunSummary();

        ValidateMeasurementPointsConfiguration();

        await _measurementPointRepository.SyncFromConfigurationAsync(_appOptions.MeasurementPoints, cancellationToken);
        var points = await _measurementPointRepository.GetActiveAsync(cancellationToken);

        summary.MeasurementPointsProcessed = points.Count;

        foreach (var point in points)
        {
            await ImportObservedAsync(point, importDate, summary, cancellationToken);
            await ImportForecastAsync(point, importDate, summary, cancellationToken);
            await ImportEnvironmentAsync(point, importDate, summary, cancellationToken);
        }

        return summary;
    }

    private async Task ImportObservedAsync(MeasurementPoint point, DateOnly importDate, ImportRunSummary summary,
        CancellationToken cancellationToken)
    {
        try
        {
            var lagDays = Math.Max(0, _appOptions.Import.ObservedDataLagDays);
            var extraLookbackDays = Math.Max(0, _appOptions.Import.ObservedLookbackExtraDays);

            ObservedDailyData? observed = null;
            DateOnly? usedDate = null;

            for (var offset = 0; offset <= extraLookbackDays; offset++)
            {
                var candidateDate = importDate.AddDays(-(lagDays + offset));
                observed = await _geosphereService.GetObservedDailyAsync(point, candidateDate, cancellationToken);
                if (observed is not null)
                {
                    usedDate = candidateDate;
                    break;
                }
            }

            if (observed is null)
            {
                summary.IncrementSkipped();
                var fromDate = importDate.AddDays(-(lagDays + extraLookbackDays));
                var toDate = importDate.AddDays(-lagDays);
                _logger.LogWarning(
                    "Keine beobachteten GeoSphere-Daten fuer Gebiet {GebietId} gefunden (Zeitraum versucht: {FromDate} bis {ToDate}).",
                    point.GebietId,
                    fromDate,
                    toDate);
                return;
            }

            var entity = new WeatherObservedDaily
            {
                GebietId = point.GebietId,
                Date = observed.Date,
                Source = observed.Source,
                TemperatureMean = observed.TemperatureMean,
                TemperatureMax = observed.TemperatureMax,
                TemperatureMin = observed.TemperatureMin,
                PrecipitationSum = observed.PrecipitationSum,
                WindSpeedMax = observed.WindSpeedMax,
                RelativeHumidityMean = observed.RelativeHumidityMean,
                WeatherCode = observed.WeatherCode,
                CreatedAt = DateTime.UtcNow
            };

            var outcome = await _weatherObservedRepository.AddIfNotExistsAsync(entity, cancellationToken);
            if (usedDate.HasValue && usedDate.Value != importDate.AddDays(-lagDays))
            {
                _logger.LogInformation(
                    "Gebiet {GebietId}: beobachtete Daten von {ObservedDate} verwendet (Rueckfall aktiv).",
                    point.GebietId,
                    usedDate.Value);
            }
            ApplyOutcome(summary, outcome);
        }
        catch (Exception ex)
        {
            summary.IncrementErrors();
            _logger.LogError(ex, "Fehler beim Import observed weather fuer Gebiet {GebietId}.", point.GebietId);
        }
    }

    private async Task ImportForecastAsync(MeasurementPoint point, DateOnly importDate, ImportRunSummary summary,
        CancellationToken cancellationToken)
    {
        try
        {
            var forecast = await _openMeteoService.GetForecastDailyAsync(point, importDate, cancellationToken);
            if (forecast is null)
            {
                summary.IncrementSkipped();
                return;
            }

            var entity = new WeatherForecastDaily
            {
                GebietId = point.GebietId,
                ForecastDate = forecast.ForecastDate,
                ForecastRunAt = forecast.ForecastRunAt,
                Source = forecast.Source,
                TemperatureMean = forecast.TemperatureMean,
                TemperatureMax = forecast.TemperatureMax,
                TemperatureMin = forecast.TemperatureMin,
                PrecipitationSum = forecast.PrecipitationSum,
                WindSpeedMax = forecast.WindSpeedMax,
                RelativeHumidityMean = forecast.RelativeHumidityMean,
                WeatherCode = forecast.WeatherCode,
                CreatedAt = DateTime.UtcNow
            };

            var outcome = await _weatherForecastRepository.AddIfNotExistsAsync(entity, cancellationToken);
            ApplyOutcome(summary, outcome);
        }
        catch (Exception ex)
        {
            summary.IncrementErrors();
            _logger.LogError(ex, "Fehler beim Import forecast weather fuer Gebiet {GebietId}.", point.GebietId);
        }
    }

    private async Task ImportEnvironmentAsync(MeasurementPoint point, DateOnly importDate, ImportRunSummary summary,
        CancellationToken cancellationToken)
    {
        try
        {
            var environment = await _openMeteoService.GetEnvironmentDailyAsync(point, importDate, cancellationToken);
            if (environment is null)
            {
                summary.IncrementSkipped();
                return;
            }

            var entity = new EnvironmentDaily
            {
                GebietId = point.GebietId,
                Date = environment.Date,
                Source = environment.Source,
                EuropeanAqi = environment.EuropeanAqi,
                Pm10 = environment.Pm10,
                Pm2_5 = environment.Pm2_5,
                NitrogenDioxide = environment.NitrogenDioxide,
                Ozone = environment.Ozone,
                AlderPollen = environment.AlderPollen,
                BirchPollen = environment.BirchPollen,
                GrassPollen = environment.GrassPollen,
                MugwortPollen = environment.MugwortPollen,
                RagweedPollen = environment.RagweedPollen,
                CreatedAt = DateTime.UtcNow
            };

            var outcome = await _environmentDailyRepository.AddIfNotExistsAsync(entity, cancellationToken);
            ApplyOutcome(summary, outcome);
        }
        catch (Exception ex)
        {
            summary.IncrementErrors();
            _logger.LogError(ex, "Fehler beim Import environment data fuer Gebiet {GebietId}.", point.GebietId);
        }
    }

    private void ValidateMeasurementPointsConfiguration()
    {
        if (_appOptions.MeasurementPoints.Count != 15)
        {
            _logger.LogWarning("Es sind {Count} Messpunkte konfiguriert. Erwartet: 15.", _appOptions.MeasurementPoints.Count);
        }
    }

    private static void ApplyOutcome(ImportRunSummary summary, SaveOutcome outcome)
    {
        if (outcome == SaveOutcome.Saved)
        {
            summary.IncrementSaved();
        }
        else
        {
            summary.IncrementSkipped();
        }
    }
}
