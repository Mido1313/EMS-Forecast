using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using WetterdatenImporter.Configuration;
using WetterdatenImporter.Data;
using WetterdatenImporter.Data.Repositories;
using WetterdatenImporter.Logging;
using WetterdatenImporter.Services;
using WetterdatenImporter.Utilities;

namespace WetterdatenImporter;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var currentDirectory = Directory.GetCurrentDirectory();

        builder.Configuration
            .SetBasePath(currentDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();

        var appOptions = builder.Configuration.GetSection("App").Get<AppOptions>();
        if (appOptions is null)
        {
            Console.Error.WriteLine("Konfigurationsbereich 'App' fehlt in appsettings.json.");
            return 1;
        }

        var databasePath = PathResolver.ResolveFromCurrentDirectory(appOptions.Database.Path);
        var logFilePath = PathResolver.ResolveFromCurrentDirectory(appOptions.Logging.FilePath);

        EnsureDirectoryExists(databasePath);
        EnsureDirectoryExists(logFilePath);

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        builder.Logging.AddProvider(new SimpleFileLoggerProvider(logFilePath, LogLevel.Information));

        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        RegisterHttpClients(builder.Services, appOptions);

        builder.Services.AddSingleton<IResilientHttpClient, ResilientHttpClient>();

        builder.Services.AddScoped<IMeasurementPointRepository, MeasurementPointRepository>();
        builder.Services.AddScoped<IWeatherObservedRepository, WeatherObservedRepository>();
        builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        builder.Services.AddScoped<IEnvironmentDailyRepository, EnvironmentDailyRepository>();

        builder.Services.AddScoped<IGeosphereService, GeosphereService>();
        builder.Services.AddScoped<IOpenMeteoService, OpenMeteoService>();
        builder.Services.AddScoped<IImportService, ImportService>();

        using var host = builder.Build();
        using var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Main");

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await EnsureDatabaseSchemaAsync(dbContext, logger, cancellationTokenSource.Token);

            var resolvedOptions = scope.ServiceProvider.GetRequiredService<IOptions<AppOptions>>().Value;
            var importDate = ResolveImportDate(resolvedOptions.Import.TimeZone, logger);

            logger.LogInformation("Starte Import fuer {Date}.", importDate);

            var importService = scope.ServiceProvider.GetRequiredService<IImportService>();
            var summary = await importService.RunDailyImportAsync(importDate, cancellationTokenSource.Token);

            logger.LogInformation(
                "Import abgeschlossen. Messpunkte: {Points}, gespeichert: {Saved}, uebersprungen: {Skipped}, Fehler: {Errors}",
                summary.MeasurementPointsProcessed,
                summary.Saved,
                summary.Skipped,
                summary.Errors);

            Console.WriteLine();
            Console.WriteLine("=== Import-Zusammenfassung ===");
            Console.WriteLine($"Messpunkte verarbeitet: {summary.MeasurementPointsProcessed}");
            Console.WriteLine($"Erfolgreich gespeichert: {summary.Saved}");
            Console.WriteLine($"Uebersprungen: {summary.Skipped}");
            Console.WriteLine($"Fehler: {summary.Errors}");

            return 0;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Import wurde abgebrochen.");
            return 2;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unbehandelter Fehler beim Importlauf.");
            return 1;
        }
    }

    private static void RegisterHttpClients(IServiceCollection services, AppOptions appOptions)
    {
        services.AddHttpClient(HttpClientNames.Geosphere, client =>
        {
            client.BaseAddress = new Uri(appOptions.Apis.Geosphere.BaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WetterdatenImporter/1.0");
        });

        services.AddHttpClient(HttpClientNames.OpenMeteoForecast, client =>
        {
            client.BaseAddress = new Uri(appOptions.Apis.OpenMeteo.ForecastBaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WetterdatenImporter/1.0");
        });

        services.AddHttpClient(HttpClientNames.OpenMeteoAirQuality, client =>
        {
            client.BaseAddress = new Uri(appOptions.Apis.OpenMeteo.AirQualityBaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WetterdatenImporter/1.0");
        });
    }

    private static DateOnly ResolveImportDate(string timeZoneId, ILogger logger)
    {
        try
        {
            return DateTimeProvider.GetLocalDateInTimeZone(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            logger.LogWarning("Zeitzone {TimeZoneId} nicht gefunden. Fallback auf UTC.", timeZoneId);
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
        catch (InvalidTimeZoneException)
        {
            logger.LogWarning("Zeitzone {TimeZoneId} ungueltig. Fallback auf UTC.", timeZoneId);
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory) == false)
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static async Task EnsureDatabaseSchemaAsync(AppDbContext dbContext, ILogger logger, CancellationToken cancellationToken)
    {
        var hasMigrations = dbContext.Database.GetMigrations().Any();
        if (hasMigrations)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            return;
        }

        logger.LogWarning("Keine EF-Migrationen gefunden. Erzeuge Tabellen direkt aus dem Modell.");

        var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();
        if (await databaseCreator.ExistsAsync(cancellationToken) == false)
        {
            await databaseCreator.CreateAsync(cancellationToken);
        }

        var measurementPointTableExists = await TableExistsAsync(dbContext, "MeasurementPoint", cancellationToken);
        if (measurementPointTableExists == false)
        {
            await databaseCreator.CreateTablesAsync(cancellationToken);
        }
    }

    private static async Task<bool> TableExistsAsync(AppDbContext dbContext, string tableName, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{tableName.Replace("'", "''")}';";

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
