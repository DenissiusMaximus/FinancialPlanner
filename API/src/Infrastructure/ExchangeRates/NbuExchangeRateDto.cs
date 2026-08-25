using System.Text.Json.Serialization;

namespace FinancialPlanner.Infrastructure.ExchangeRates;

public sealed class NbuExchangeRateDto
{
    [JsonPropertyName("r030")]
    public int NumericCode { get; set; }

    [JsonPropertyName("txt")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("cc")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonPropertyName("exchangedate")]
    public string ExchangeDate { get; set; } = string.Empty;
}
