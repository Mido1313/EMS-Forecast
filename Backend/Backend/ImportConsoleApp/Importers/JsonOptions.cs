namespace ImportConsoleApp.Importers;

using System.Text.Json;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}