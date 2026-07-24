namespace ImportConsoleApp.Importers;

using ClosedXML.Excel;
using Core.Contracts;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class PostalCodeImporter
{
    public static async Task<(Dictionary<string, PostalCode> PostalCodes, Dictionary<string, Municipality> Municipalities)> ImportAsync(
        IUnitOfWork uow, string dataPath, Dictionary<int, District> districtsByAreaId)
    {
        var filePath = Path.Combine(dataPath, "PLZ_Liste.xlsx");
        Console.WriteLine($"[PostalCode] lese {filePath}");

        using var workbook = new XLWorkbook(filePath);

        var existingPostalCodes = await uow.PostalCodeRepository.GetNoTrackingAsync();
        var plzLookup = existingPostalCodes.ToDictionary(p => p.Plz, p => p);

        var gebieteSheet = workbook.Worksheet("PLZ_Gebiete");
        var newPostalCodes = 0;

        foreach (var row in gebieteSheet.RowsUsed().Skip(1))
        {
            var plz = row.Cell(1).GetValue<int>().ToString();
            var ortsname = row.Cell(2).GetString().Trim();
            var gebietId = row.Cell(3).GetValue<int>();

            if (plzLookup.ContainsKey(plz)) continue;

            if (!districtsByAreaId.TryGetValue(gebietId, out var district))
            {
                Console.WriteLine($"[PostalCode] WARNUNG: Gebiet_ID {gebietId} fuer PLZ {plz} unbekannt, uebersprungen.");
                continue;
            }

            var postalCode = new PostalCode { Plz = plz, CityName = ortsname, DistrictId = district.Id };
            await uow.PostalCodeRepository.AddAsync(postalCode);
            plzLookup[plz] = postalCode;
            newPostalCodes++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[PostalCode] {newPostalCodes} neu, {plzLookup.Count} gesamt.");

        var existingMunicipalities = await uow.MunicipalityRepository.GetNoTrackingAsync();
        var municipalityLookup = existingMunicipalities.ToDictionary(m => $"{m.PostalCodeId}|{m.MunicipalityName}", m => m);

        var gemeindenSheet = workbook.Worksheet("PLZ_Gemeinden");
        var newMunicipalities = 0;

        foreach (var row in gemeindenSheet.RowsUsed().Skip(1))
        {
            var plz = row.Cell(1).GetValue<int>().ToString();
            var gemeindeName = row.Cell(2).GetString().Trim();
            var key = $"{plz}|{gemeindeName}";

            if (municipalityLookup.ContainsKey(key)) continue;

            if (!plzLookup.ContainsKey(plz))
            {
                Console.WriteLine($"[Municipality] WARNUNG: PLZ {plz} fuer '{gemeindeName}' unbekannt, uebersprungen.");
                continue;
            }

            var municipality = new Municipality { PostalCodeId = plz, MunicipalityName = gemeindeName };
            await uow.MunicipalityRepository.AddAsync(municipality);
            municipalityLookup[key] = municipality;
            newMunicipalities++;
        }

        await uow.SaveChangesAsync();
        Console.WriteLine($"[Municipality] {newMunicipalities} neu, {municipalityLookup.Count} gesamt.");

        return (plzLookup, municipalityLookup);
    }
}