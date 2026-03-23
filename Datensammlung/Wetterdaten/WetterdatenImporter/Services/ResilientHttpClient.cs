using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WetterdatenImporter.Configuration;

namespace WetterdatenImporter.Services;

public sealed class ResilientHttpClient : IResilientHttpClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ImportOptions _importOptions;
    private readonly ILogger<ResilientHttpClient> _logger;

    public ResilientHttpClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AppOptions> appOptions,
        ILogger<ResilientHttpClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _importOptions = appOptions.Value.Import;
        _logger = logger;
    }

    public async Task<JsonDocument> GetJsonAsync(string clientName, string requestUri, CancellationToken cancellationToken)
    {
        var attempts = Math.Max(1, _importOptions.RetryCount);
        var delay = TimeSpan.FromSeconds(Math.Max(1, _importOptions.RetryDelaySeconds));
        var timeout = TimeSpan.FromSeconds(Math.Max(5, _importOptions.TimeoutSeconds));

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            try
            {
                var client = _httpClientFactory.CreateClient(clientName);
                using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                }

                var statusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (IsTransientStatusCode(statusCode) && attempt < attempts)
                {
                    var retryDelay = GetRetryDelay(response, delay * attempt);
                    _logger.LogWarning(
                        "Transient HTTP status {StatusCode} bei {Uri}. Retry {Attempt}/{MaxAttempts} in {RetryDelayMs} ms. Body: {Body}",
                        (int)statusCode,
                        requestUri,
                        attempt,
                        attempts,
                        (int)retryDelay.TotalMilliseconds,
                        Truncate(responseBody, 600));

                    await Task.Delay(retryDelay, cancellationToken);
                    continue;
                }

                throw new HttpRequestException(
                    $"HTTP {(int)statusCode} ({statusCode}) bei {requestUri}. Body: {Truncate(responseBody, 1000)}",
                    null,
                    statusCode);
            }
            catch (Exception ex) when (IsTransientException(ex, cancellationToken) && attempt < attempts)
            {
                _logger.LogWarning(ex, "HTTP Fehler bei {Uri}. Retry {Attempt}/{MaxAttempts}.", requestUri, attempt, attempts);
                await Task.Delay(delay * attempt, cancellationToken);
            }
        }

        throw new HttpRequestException($"HTTP-Aufruf fehlgeschlagen: {requestUri}");
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.RequestTimeout
               || (int)statusCode == 429
               || (int)statusCode >= 500;
    }

    private static bool IsTransientException(Exception exception, CancellationToken cancellationToken)
    {
        return exception switch
        {
            HttpRequestException httpException when httpException.StatusCode.HasValue == false => true,
            HttpRequestException httpException when IsTransientStatusCode(httpException.StatusCode!.Value) => true,
            TaskCanceledException when cancellationToken.IsCancellationRequested == false => true,
            _ => false
        };
    }

    private static string Truncate(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
        {
            return input;
        }

        return input.Substring(0, maxLength) + "...";
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, TimeSpan fallback)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is not null && retryAfter.Delta.Value > TimeSpan.Zero)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter?.Date is not null)
        {
            var delta = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            if (delta > TimeSpan.Zero)
            {
                return delta;
            }
        }

        if (response.Headers.TryGetValues("ratelimit-reset", out var resetValues))
        {
            var first = resetValues.FirstOrDefault();
            if (int.TryParse(first, out var secondsToReset) && secondsToReset > 0)
            {
                var candidate = TimeSpan.FromSeconds(secondsToReset);
                if (candidate > fallback)
                {
                    return candidate;
                }
            }
        }

        return fallback;
    }
}
