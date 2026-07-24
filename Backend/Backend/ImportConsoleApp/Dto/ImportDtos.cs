namespace ImportConsoleApp.Dto;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class GebietsMappingDto
{
    [JsonPropertyName("nameToArea")]
    public Dictionary<string, int> NameToArea { get; set; } = new();
}

public class AusflugszieleRootDto
{
    [JsonPropertyName("ausflugsziele")]
    public List<AusflugszielDto> Ausflugsziele { get; set; } = new();
}

public class AusflugszielDto
{
    public string Name { get; set; } = "";
    public string Ort { get; set; } = "";
    public string Plz { get; set; } = "";
    public string Art { get; set; } = "";
    public decimal Risikostufe { get; set; }
    public List<string> Saison { get; set; } = new();
}

public class BevoelkerungsdatenDto
{
    public int Plz { get; set; }
    public string Gemeinde { get; set; } = "";
    public string? Bezeichnung { get; set; }
    public int GesBev { get; set; }
    public double? Unter15 { get; set; }
    public double? Ueber65 { get; set; }
    public double? Ewt15 { get; set; }
    public double? Abl1564 { get; set; }
    public int? PrivHaushalt { get; set; }
    public double? AvgGroesse { get; set; }
    public int? FamilienHaushalt { get; set; }

    [JsonPropertyName("male_agegroup_1")] public int? MaleAgegroup1 { get; set; }
    [JsonPropertyName("male_agegroup_2")] public int? MaleAgegroup2 { get; set; }
    [JsonPropertyName("male_agegroup_3")] public int? MaleAgegroup3 { get; set; }
    [JsonPropertyName("male_agegroup_4")] public int? MaleAgegroup4 { get; set; }

    [JsonPropertyName("female_agegroup_1")] public int? FemaleAgegroup1 { get; set; }
    [JsonPropertyName("female_agegroup_2")] public int? FemaleAgegroup2 { get; set; }
    [JsonPropertyName("female_agegroup_3")] public int? FemaleAgegroup3 { get; set; }
    [JsonPropertyName("female_agegroup_4")] public int? FemaleAgegroup4 { get; set; }

    [JsonPropertyName("total_agegroup_1")] public int? TotalAgegroup1 { get; set; }
    [JsonPropertyName("total_agegroup_2")] public int? TotalAgegroup2 { get; set; }
    [JsonPropertyName("total_agegroup_3")] public int? TotalAgegroup3 { get; set; }
    [JsonPropertyName("total_agegroup_4")] public int? TotalAgegroup4 { get; set; }
}

public class EventsRootDto
{
    [JsonPropertyName("events")]
    public List<EventJsonDto> Events { get; set; } = new();
}

public class EventJsonDto
{
    public string Name { get; set; } = "";
    public string Ort { get; set; } = "";
    public string Plz { get; set; } = "";
    [JsonPropertyName("start_datum")] public string StartDatum { get; set; } = "";
    [JsonPropertyName("end_datum")] public string EndDatum { get; set; } = "";
}

public class PflegeheimeRootDto
{
    [JsonPropertyName("pflegeheime")]
    public List<PflegeheimDto> Pflegeheime { get; set; } = new();
}

public class PflegeheimDto
{
    public string Name { get; set; } = "";
    public string Ort { get; set; } = "";
    public string Plz { get; set; } = "";
    public int? Pflegeplaetze { get; set; }
}

public class FeiertagFerienDto
{
    public string Typ { get; set; } = "";
    public string Name { get; set; } = "";
    public string Start { get; set; } = "";
    public string Ende { get; set; } = "";
}