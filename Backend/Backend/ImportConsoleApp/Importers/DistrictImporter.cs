namespace ImportConsoleApp.Importers;

using Core.Contracts;
using Core.Entities;
using ImportConsoleApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public static class DistrictImporter
{
    public static async Task<Dictionary<int, District>> ImportAsync(IUnitOfWork uow, string dataPath)
    {
        var filePath = Path.Combine(dataPath, "Verkehrsdaten", "gebiets_mapping.json");
        Console.WriteLine($"[District] lese {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var dto = JsonSerializer.Deserialize<GebietsMappingDto>(json, JsonOptions.Default)
                  ?? throw new InvalidOperationException("gebiets_mapping.json konnte nicht gelesen werden.");

        var existing = await uow.DistrictRepository.GetNoTrackingAsync();
        var lookup = existing.ToDictionary(d => d.DistrictId, d => d);

        var newCount = 0;
        foreach (var (name, areaId) in dto.NameToArea)
        {
            if (lookup.ContainsKey(areaId)) continue;

            var district = new District { DistrictId = areaId, DistrictName = name };
            await uow.DistrictRepository.AddAsync(district);
            lookup[areaId] = district;
            newCount++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[District] {newCount} neu, {lookup.Count} gesamt.");
        return lookup;
    }
}