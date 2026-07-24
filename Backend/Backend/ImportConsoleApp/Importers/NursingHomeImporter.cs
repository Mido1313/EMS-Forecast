namespace ImportConsoleApp.Importers;

using Core.Contracts;
using Core.Entities;
using ImportConsoleApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class NursingHomeImporter
{
    public static async Task ImportAsync(IUnitOfWork uow, string dataPath, Dictionary<string, PostalCode> postalCodesByPlz)
    {
        var filePath = Path.Combine(dataPath, "Pflegeheime", "Pflegeheime.json");
        Console.WriteLine($"[NursingHome] lese {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var root = JsonSerializer.Deserialize<PflegeheimeRootDto>(json, JsonOptions.Default) ?? new PflegeheimeRootDto();

        var existing = await uow.NursingHomeRepository.GetNoTrackingAsync();
        var existingNames = existing.Select(n => $"{n.PostalCodeId}|{n.Name}").ToHashSet();

        var newCount = 0; var skipped = 0;

        foreach (var entry in root.Pflegeheime)
        {
            var key = $"{entry.Plz}|{entry.Name}";
            if (existingNames.Contains(key)) continue;

            if (!postalCodesByPlz.ContainsKey(entry.Plz))
            {
                Console.WriteLine($"[NursingHome] WARNUNG: PLZ {entry.Plz} fuer '{entry.Name}' unbekannt, uebersprungen.");
                skipped++; continue;
            }

            var nursingHome = new NursingHome { PostalCodeId = entry.Plz, Name = entry.Name, BedCount = entry.Pflegeplaetze };
            await uow.NursingHomeRepository.AddAsync(nursingHome);
            existingNames.Add(key);
            newCount++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[NursingHome] {newCount} neu, {skipped} uebersprungen.");
    }
}