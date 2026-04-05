using System.Text.Json.Serialization;

namespace API.Services.Aim;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AimSortBy
{
    Amount,
    Priority,
}