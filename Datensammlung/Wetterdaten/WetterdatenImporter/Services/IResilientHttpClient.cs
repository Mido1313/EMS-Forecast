using System.Text.Json;

namespace WetterdatenImporter.Services;

public interface IResilientHttpClient
{
    Task<JsonDocument> GetJsonAsync(string clientName, string requestUri, CancellationToken cancellationToken);
}
