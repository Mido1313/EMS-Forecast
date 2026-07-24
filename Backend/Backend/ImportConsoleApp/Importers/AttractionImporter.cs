namespace ImportConsoleApp.Importers;

using Core.Contracts;
using Core.Entities;
using ImportConsoleApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class AttractionImporter
{
    public static async Task ImportAsync(IUnitOfWork uow, string dataPath, Dictionary<string, PostalCode> postalCodesByPlz)
    {
        var filePath = Path.Combine(dataPath, "BergeSeen", "Ausflugziele.json");
        Console.WriteLine($"[Attraction] lese {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var root = JsonSerializer.Deserialize<AusflugszieleRootDto>(json, JsonOptions.Default) ?? new AusflugszieleRootDto();

        var existing = await uow.AttractionRepository.GetNoTrackingAsync();
        var existingNames = existing.Select(a => $"{a.PostalCodeId}|{a.Name}").ToHashSet();

        var newCount = 0; var skipped = 0;

        foreach (var entry in root.Ausflugsziele)
        {
            var key = $"{entry.Plz}|{entry.Name}";
            if (existingNames.Contains(key)) continue;

            if (!postalCodesByPlz.ContainsKey(entry.Plz))
            {
                Console.WriteLine($"[Attraction] WARNUNG: PLZ {entry.Plz} fuer '{entry.Name}' unbekannt, uebersprungen.");
                skipped++; continue;
            }

            var isSommer = entry.Saison.Any(s => s.Equals("Sommer", StringComparison.OrdinalIgnoreCase));
            var isWinter = entry.Saison.Any(s => s.Equals("Winter", StringComparison.OrdinalIgnoreCase));

            var attraction = new Attraction
            {
                PostalCodeId = entry.Plz,
                Name = entry.Name,
                RiskSummer = isSommer ? entry.Risikostufe : null,
                RiskWinter = isWinter ? entry.Risikostufe : null,
            };

            await uow.AttractionRepository.AddAsync(attraction);
            existingNames.Add(key);
            newCount++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[Attraction] {newCount} neu, {skipped} uebersprungen.");
    }
}