using Mapster;

namespace FinancialPlanner.Application.Common.Mapping;

public static class PatchMapperConfig
{
    public static readonly TypeAdapterConfig Instance = new();

    public static void Configure()
    {
        Instance.Default.IgnoreNullValues(true);
        Instance.Scan(typeof(PatchMapperConfig).Assembly);
    }
}
