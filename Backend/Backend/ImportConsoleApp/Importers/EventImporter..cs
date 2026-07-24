namespace ImportConsoleApp.Importers;

using Core.Contracts;
using Core.Entities;
using ImportConsoleApp.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

public static class EventImporter
{
    public static async Task ImportAsync(IUnitOfWork uow, string dataPath, Dictionary<string, PostalCode> postalCodesByPlz)
    {
        var filePath = Path.Combine(dataPath, "Events", "Events.json");
        Console.WriteLine($"[Event] lese {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var root = JsonSerializer.Deserialize<EventsRootDto>(json, JsonOptions.Default) ?? new EventsRootDto();

        var existing = await uow.EventRepository.GetNoTrackingAsync();
        var existingNames = existing.Select(e => $"{e.PostalCodeId}|{e.Name}|{e.DateFrom:yyyy-MM-dd}").ToHashSet();

        var newCount = 0; var skipped = 0;

        foreach (var entry in root.Events)
        {
            if (!postalCodesByPlz.ContainsKey(entry.Plz))
            {
                Console.WriteLine($"[Event] WARNUNG: PLZ {entry.Plz} fuer '{entry.Name}' unbekannt, uebersprungen.");
                skipped++; continue;
            }

            if (!DateTime.TryParseExact(entry.StartDatum, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateFrom) ||
                !DateTime.TryParseExact(entry.EndDatum, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTo))
            {
                skipped++; continue;
            }

            var key = $"{entry.Plz}|{entry.Name}|{dateFrom:yyyy-MM-dd}";
            if (existingNames.Contains(key)) continue;

            var evt = new Event { PostalCodeId = entry.Plz, Name = entry.Name, DateFrom = dateFrom, DateTo = dateTo };
            await uow.EventRepository.AddAsync(evt);
            existingNames.Add(key);
            newCount++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[Event] {newCount} neu, {skipped} uebersprungen.");
    }
}