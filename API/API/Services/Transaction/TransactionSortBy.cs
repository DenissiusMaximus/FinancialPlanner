using System.Text.Json.Serialization;

namespace API.Services.Transaction;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionSortBy
{
    Date,
    Amount
}
