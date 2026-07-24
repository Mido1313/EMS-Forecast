using Core.Contracts;
using ImportConsoleApp.Importers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using System;
using System.IO;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection fehlt.");
var dataPath = configuration["ImportSettings:DataPath"]
               ?? throw new InvalidOperationException("ImportSettings:DataPath fehlt.");

if (!Directory.Exists(dataPath))
{
    Console.Error.WriteLine($"Datenordner nicht gefunden: {dataPath}");
    return 1;
}

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
services.AddPersistence();

await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();
var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

try
{
    Console.WriteLine("=== Import startet ===");
    Console.WriteLine($"Datenquelle: {dataPath}\n");

    var districts = await DistrictImporter.ImportAsync(uow, dataPath);
    var (postalCodes, municipalities) = await PostalCodeImporter.ImportAsync(uow, dataPath, districts);
    await PopulationImporter.ImportAsync(uow, dataPath, municipalities);
    await NursingHomeImporter.ImportAsync(uow, dataPath, postalCodes);
    await AttractionImporter.ImportAsync(uow, dataPath, postalCodes);
    await EventImporter.ImportAsync(uow, dataPath, postalCodes);
    await PublicHolidayImporter.ImportAsync(uow, dataPath);
    var (incidentTypes, locationTypes) = await IncidentReferenceImporter.ImportAsync(uow, dataPath);
    await IncidentImporter.ImportAsync(uow, dataPath, postalCodes, incidentTypes, locationTypes);

    Console.WriteLine("\n=== Import abgeschlossen ===");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("\n=== Import fehlgeschlagen ===");
    Console.Error.WriteLine(ex);
    return 1;
}