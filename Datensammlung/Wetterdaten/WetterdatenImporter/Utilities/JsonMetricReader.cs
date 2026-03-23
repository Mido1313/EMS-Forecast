using System.Globalization;
using System.Text.Json;

namespace WetterdatenImporter.Utilities;

public static class JsonMetricReader
{
    private static readonly string[] DatePropertyCandidates = ["time", "date", "dates", "timestamp", "timestamps"];

    public static JsonElement GetDailyContainerOrRoot(JsonElement root)
    {
        if (TryFindPropertyRecursive(root, "daily", out var daily) && daily.ValueKind == JsonValueKind.Object)
        {
            return daily;
        }

        if (TryFindPropertyRecursive(root, "data", out var data) && data.ValueKind == JsonValueKind.Object)
        {
            return data;
        }

        return root;
    }

    public static int FindDateIndex(JsonElement container, DateOnly targetDate)
    {
        foreach (var key in DatePropertyCandidates)
        {
            if (TryFindPropertyRecursive(container, key, out var dateElement) == false)
            {
                continue;
            }

            if (dateElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var index = 0;
            foreach (var item in dateElement.EnumerateArray())
            {
                if (TryParseDate(item, out var parsedDate) && parsedDate == targetDate)
                {
                    return index;
                }

                index++;
            }
        }

        return -1;
    }

    public static DateOnly? ReadDateAtIndex(JsonElement container, int index)
    {
        foreach (var key in DatePropertyCandidates)
        {
            if (TryFindPropertyRecursive(container, key, out var dateElement) == false)
            {
                continue;
            }

            if (dateElement.ValueKind != JsonValueKind.Array || index < 0 || index >= dateElement.GetArrayLength())
            {
                continue;
            }

            var item = dateElement[index];
            if (TryParseDate(item, out var parsedDate))
            {
                return parsedDate;
            }
        }

        return null;
    }

    public static double? ReadDoubleByCandidates(JsonElement container, int dateIndex, params string[] metricCandidates)
    {
        foreach (var candidate in metricCandidates)
        {
            if (TryFindPropertyRecursive(container, candidate, out var metricElement) == false)
            {
                continue;
            }

            var value = ReadDoubleValue(metricElement, dateIndex);
            if (value.HasValue)
            {
                return value;
            }
        }

        return null;
    }

    public static int? ReadIntByCandidates(JsonElement container, int dateIndex, params string[] metricCandidates)
    {
        foreach (var candidate in metricCandidates)
        {
            if (TryFindPropertyRecursive(container, candidate, out var metricElement) == false)
            {
                continue;
            }

            var value = ReadIntValue(metricElement, dateIndex);
            if (value.HasValue)
            {
                return value;
            }
        }

        return null;
    }

    public static bool TryFindPropertyRecursive(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) || property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                if (TryFindPropertyRecursive(property.Value, propertyName, out value))
                {
                    return true;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in element.EnumerateArray())
            {
                if (TryFindPropertyRecursive(child, propertyName, out value))
                {
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static double? ReadDoubleValue(JsonElement element, int dateIndex)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            if (dateIndex >= 0 && dateIndex < element.GetArrayLength())
            {
                return ReadDoubleValue(element[dateIndex], -1);
            }

            foreach (var item in element.EnumerateArray())
            {
                var candidate = ReadDoubleValue(item, -1);
                if (candidate.HasValue)
                {
                    return candidate;
                }
            }

            return null;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            if (TryFindPropertyRecursive(element, "values", out var values))
            {
                return ReadDoubleValue(values, dateIndex);
            }

            foreach (var property in element.EnumerateObject())
            {
                var candidate = ReadDoubleValue(property.Value, dateIndex);
                if (candidate.HasValue)
                {
                    return candidate;
                }
            }

            return null;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var number))
        {
            return number;
        }

        if (element.ValueKind == JsonValueKind.String
            && double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static int? ReadIntValue(JsonElement element, int dateIndex)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            if (dateIndex >= 0 && dateIndex < element.GetArrayLength())
            {
                return ReadIntValue(element[dateIndex], -1);
            }

            foreach (var item in element.EnumerateArray())
            {
                var candidate = ReadIntValue(item, -1);
                if (candidate.HasValue)
                {
                    return candidate;
                }
            }

            return null;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            if (TryFindPropertyRecursive(element, "values", out var values))
            {
                return ReadIntValue(values, dateIndex);
            }

            foreach (var property in element.EnumerateObject())
            {
                var candidate = ReadIntValue(property.Value, dateIndex);
                if (candidate.HasValue)
                {
                    return candidate;
                }
            }

            return null;
        }

        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetInt32(out var intValue))
            {
                return intValue;
            }

            if (element.TryGetDouble(out var doubleValue))
            {
                return Convert.ToInt32(Math.Round(doubleValue));
            }
        }

        if (element.ValueKind == JsonValueKind.String
            && int.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static bool TryParseDate(JsonElement item, out DateOnly date)
    {
        if (item.ValueKind == JsonValueKind.String)
        {
            var raw = item.GetString();
            if (DateOnly.TryParse(raw, out date))
            {
                return true;
            }

            if (DateTimeOffset.TryParse(raw, out var dateTimeOffset))
            {
                date = DateOnly.FromDateTime(dateTimeOffset.DateTime);
                return true;
            }

            if (DateTime.TryParse(raw, out var dateTime))
            {
                date = DateOnly.FromDateTime(dateTime);
                return true;
            }
        }

        date = default;
        return false;
    }
}
