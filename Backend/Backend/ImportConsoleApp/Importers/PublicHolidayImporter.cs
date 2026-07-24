namespace ImportConsoleApp.Importers;

using Core.Contracts;
using Core.Entities;
using ImportConsoleApp.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

public static class PublicHolidayImporter
{
    public static async Task ImportAsync(IUnitOfWork uow, string dataPath)
    {
        var filePath = Path.Combine(dataPath, "FeiertageFerien", "datamodeler.json");
        Console.WriteLine($"[PublicHoliday] lese {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var entries = JsonSerializer.Deserialize<List<FeiertagFerienDto>>(json, JsonOptions.Default) ?? new List<FeiertagFerienDto>();

        var existing = await uow.PublicHolidayRepository.GetNoTrackingAsync();
        var existingNames = existing.Select(p => $"{p.Name}|{p.Date:yyyy-MM-dd}").ToHashSet();

        var newCount = 0; var skipped = 0;

        foreach (var entry in entries)
        {
            if (!DateTime.TryParseExact(entry.Start, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            {
                skipped++; continue;
            }

            DateTime? periodEnd = null;
            var isFerien = entry.Typ.Equals("Ferien", StringComparison.OrdinalIgnoreCase);
            if (isFerien && DateTime.TryParseExact(entry.Ende, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ende))
                periodEnd = ende;

            var key = $"{entry.Name}|{start:yyyy-MM-dd}";
            if (existingNames.Contains(key)) continue;

            var holiday = new PublicHoliday { Name = entry.Name, Date = start, PeriodEnd = periodEnd, IsSchoolBreak = isFerien };
            await uow.PublicHolidayRepository.AddAsync(holiday);
            existingNames.Add(key);
            newCount++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[PublicHoliday] {newCount} neu, {skipped} uebersprungen.");
    }
}