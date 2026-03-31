using System.Text.Json.Serialization;

namespace API.Services;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionSortBy
{
    Date,
    Amount
}
