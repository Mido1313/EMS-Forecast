namespace ImportConsoleApp.Importers;

using ClosedXML.Excel;
using Core.Contracts;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

public static class IncidentImporter
{
    private static readonly (string File, string Sheet)[] Sources =
    {
        ("Mockdaten/ems_mockdaten_lukas.xlsx", "incidents"),
        ("Mockdaten/ems_mockdaten_merjem.xlsx", "incidents"),
        ("Mockdaten/ems_mockdaten_mido.xlsx", "incidents"),
        ("Mockdaten/ems_mockdaten_zusatz.xlsx", "Sheet1"),
    };

    public static async Task ImportAsync(
        IUnitOfWork uow, string dataPath,
        Dictionary<string, PostalCode> postalCodesByPlz,
        Dictionary<int, IncidentType> incidentTypesBySourceId,
        Dictionary<int, LocationType> locationTypesBySourceId)
    {
        var existing = await uow.IncidentRepository.GetNoTrackingAsync();
        var existingKeys = existing
            .Select(i => $"{i.PostalCodeId}|{i.Timestamp:O}|{i.IncidentTypeId}|{i.LocationTypeId}|{i.Age}")
            .ToHashSet();

        var totalNew = 0; var totalSkipped = 0;
        var toInsert = new List<Incident>();

        foreach (var (file, sheetName) in Sources)
        {
            var filePath = Path.Combine(dataPath, file);
            Console.WriteLine($"[Incident] lese {filePath} ({sheetName})");

            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheet(sheetName);

            var fileNew = 0; var fileSkipped = 0;

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var dateCell = row.Cell(1);
                if (dateCell.IsEmpty()) continue;

                var date = dateCell.GetDateTime();
                var time = row.Cell(2).GetValue<TimeSpan>();
                var plz = row.Cell(3).GetValue<int>().ToString();
                var incidentTypeSourceId = row.Cell(4).GetValue<int>();
                var age = row.Cell(6).IsEmpty() ? (int?)null : row.Cell(6).GetValue<int>();
                var locationTypeSourceId = row.Cell(7).GetValue<int>();

                if (!postalCodesByPlz.TryGetValue(plz, out var postalCode)) { fileSkipped++; continue; }
                if (!incidentTypesBySourceId.TryGetValue(incidentTypeSourceId, out var incidentType) ||
                    !locationTypesBySourceId.TryGetValue(locationTypeSourceId, out var locationType))
                { fileSkipped++; continue; }

                var timestamp = date.Date + time;
                var key = $"{postalCode.Plz}|{timestamp:O}|{incidentType.Id}|{locationType.Id}|{age}";
                if (!existingKeys.Add(key)) continue;

                toInsert.Add(new Incident
                {
                    PostalCodeId = postalCode.Plz,
                    LocationTypeId = locationType.Id,
                    IncidentTypeId = incidentType.Id,
                    Timestamp = timestamp,
                    Age = age,
                });
                fileNew++;
            }

            Console.WriteLine($"[Incident]   {fileNew} neu, {fileSkipped} uebersprungen in {file}");
            totalNew += fileNew; totalSkipped += fileSkipped;

            if (toInsert.Count >= 1000)
            {
                await uow.IncidentRepository.AddRangeAsync(toInsert);
                await uow.SaveChangesAsync();
                toInsert.Clear();
            }
        }

        if (toInsert.Count > 0)
        {
            await uow.IncidentRepository.AddRangeAsync(toInsert);
            await uow.SaveChangesAsync();
        }

        Console.WriteLine($"[Incident] GESAMT: {totalNew} neu, {totalSkipped} uebersprungen (unbekannte PLZ/Referenz).");
    }
}