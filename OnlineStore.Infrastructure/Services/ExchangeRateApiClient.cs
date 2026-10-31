using System.Text.Json;
using Microsoft.Extensions.Logging;
using OnlineStore.Core.Exceptions;
using OnlineStore.Core.Interfaces;

namespace OnlineStore.Infrastructure.Services;

public class ExchangeRateApiClient : ICurrencyConverter
{
    private readonly HttpClient _http;
    private readonly ILogger<ExchangeRateApiClient> _logger;

    public ExchangeRateApiClient(HttpClient http, ILogger<ExchangeRateApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<decimal> GetRateFromUsdAsync(string currencyCode, CancellationToken ct = default)
    {
        var code = (currencyCode ?? string.Empty).Trim().ToUpperInvariant();
        if (code.Length != 3 || !code.All(char.IsLetter))
            throw new BadRequestException("Currency must be a 3-letter ISO code, e.g. EUR, GEL, GBP.");
        if (code == "USD") return 1m;

        try
        {
            using var response = await _http.GetAsync("v6/latest/USD", ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (doc.RootElement.TryGetProperty("rates", out var rates) &&
                rates.TryGetProperty(code, out var rate))
            {
                return rate.GetDecimal();
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException
                                   && !ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Exchange-rate API call failed.");
            throw new ExternalServiceException("The exchange-rate service is currently unavailable.");
        }

        throw new BadRequestException($"Currency '{code}' is not supported.");
    }
}
