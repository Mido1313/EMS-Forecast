namespace ImportConsoleApp.Importers;

using Core.Contracts;
using Core.Entities;
using ImportConsoleApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class PopulationImporter
{
    public static async Task ImportAsync(IUnitOfWork uow, string dataPath, Dictionary<string, Municipality> municipalitiesByPlzAndName)
    {
        var filePath = Path.Combine(dataPath, "Bevoelkerungsdaten", "Bevoelkerungsdaten.json");
        Console.WriteLine($"[Population] lese {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var entries = JsonSerializer.Deserialize<List<BevoelkerungsdatenDto>>(json, JsonOptions.Default) ?? new List<BevoelkerungsdatenDto>();

        var existing = await uow.PopulationRepository.GetNoTrackingAsync();
        var existingMunicipalityIds = existing.Select(p => p.MunicipalityId).ToHashSet();

        var newCount = 0;
        var skipped = 0;

        foreach (var entry in entries)
        {
            var key = $"{entry.Plz}|{entry.Gemeinde}";
            if (!municipalitiesByPlzAndName.TryGetValue(key, out var municipality)) { skipped++; continue; }
            if (existingMunicipalityIds.Contains(municipality.Id)) continue;

            var ageStructure = JsonSerializer.Serialize(new
            {
                entry.Unter15,
                entry.Ueber65,
                entry.Ewt15,
                entry.Abl1564,
                entry.PrivHaushalt,
                entry.AvgGroesse,
                entry.FamilienHaushalt,
                entry.MaleAgegroup1,
                entry.MaleAgegroup2,
                entry.MaleAgegroup3,
                entry.MaleAgegroup4,
                entry.FemaleAgegroup1,
                entry.FemaleAgegroup2,
                entry.FemaleAgegroup3,
                entry.FemaleAgegroup4,
                entry.TotalAgegroup1,
                entry.TotalAgegroup2,
                entry.TotalAgegroup3,
                entry.TotalAgegroup4,
            });

            var population = new Population
            {
                MunicipalityId = municipality.Id,
                ResidentCount = entry.GesBev,
                AgeStructure = ageStructure,
            };

            await uow.PopulationRepository.AddAsync(population);
            existingMunicipalityIds.Add(municipality.Id);
            newCount++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[Population] {newCount} neu, {skipped} ohne passende Gemeinde uebersprungen.");
    }
}