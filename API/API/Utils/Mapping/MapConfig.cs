using System.Reflection;
using Mapster;

namespace API.Extensions;

public static class MapConfig
{
    public static readonly TypeAdapterConfig PatchConfig = new TypeAdapterConfig();

    public static void Configure()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        PatchConfig.Default.IgnoreNullValues(true);

        config.Scan(Assembly.GetExecutingAssembly());
    }
}
