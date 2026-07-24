namespace ImportConsoleApp.Importers;

using ClosedXML.Excel;
using Core.Contracts;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class IncidentReferenceImporter
{
    public static async Task<(Dictionary<int, IncidentType> IncidentTypes, Dictionary<int, LocationType> LocationTypes)> ImportAsync(
        IUnitOfWork uow, string dataPath)
    {
        var filePath = Path.Combine(dataPath, "Mockdaten_Muster.xlsx");
        Console.WriteLine($"[IncidentType/LocationType] lese {filePath}");

        using var workbook = new XLWorkbook(filePath);

        var existingTypes = await uow.IncidentTypeRepository.GetNoTrackingAsync();
        var typesByName = existingTypes.ToDictionary(t => t.IncidentTypeName, t => t);
        var incidentTypeLookup = new Dictionary<int, IncidentType>();

        var typeSheet = workbook.Worksheet("incident_type");
        var newTypes = 0;

        foreach (var row in typeSheet.RowsUsed().Skip(1))
        {
            var sourceId = row.Cell(1).GetValue<int>();
            var name = row.Cell(2).GetString().Trim();
            var severityMean = row.Cell(3).IsEmpty() ? (decimal?)null : row.Cell(3).GetValue<decimal>();
            var severityMin = row.Cell(4).IsEmpty() ? (decimal?)null : row.Cell(4).GetValue<decimal>();
            var severityMax = row.Cell(5).IsEmpty() ? (decimal?)null : row.Cell(5).GetValue<decimal>();

            if (!typesByName.TryGetValue(name, out var incidentType))
            {
                incidentType = new IncidentType { IncidentTypeName = name, SeverityMean = severityMean, SeverityMin = severityMin, SeverityMax = severityMax };
                await uow.IncidentTypeRepository.AddAsync(incidentType);
                typesByName[name] = incidentType;
                newTypes++;
            }
            incidentTypeLookup[sourceId] = incidentType;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[IncidentType] {newTypes} neu, {incidentTypeLookup.Count} Zuordnungen.");

        var existingLocationTypes = await uow.LocationTypeRepository.GetNoTrackingAsync();
        var locationTypesByName = existingLocationTypes.ToDictionary(l => l.LocationTypeName, l => l);
        var locationTypeLookup = new Dictionary<int, LocationType>();

        var locationSheet = workbook.Worksheet("location_type");
        var newLocationTypes = 0;

        foreach (var row in locationSheet.RowsUsed().Skip(1))
        {
            var sourceId = row.Cell(1).GetValue<int>();
            var name = row.Cell(2).GetString().Trim();

            if (!locationTypesByName.TryGetValue(name, out var locationType))
            {
                locationType = new LocationType { LocationTypeName = name };
                await uow.LocationTypeRepository.AddAsync(locationType);
                locationTypesByName[name] = locationType;
                newLocationTypes++;
            }
            locationTypeLookup[sourceId] = locationType;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[LocationType] {newLocationTypes} neu, {locationTypeLookup.Count} Zuordnungen.");

        return (incidentTypeLookup, locationTypeLookup);
    }
}