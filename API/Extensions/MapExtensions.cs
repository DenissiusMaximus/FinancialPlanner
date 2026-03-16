using System;
using Mapster;

namespace API.Utils.Map;

public static class MapConfig
{
    public static readonly TypeAdapterConfig PatchConfig = new TypeAdapterConfig();

    static MapConfig()
    {
        PatchConfig.Default.IgnoreNullValues(true);
    }
}

public static class MappingExtensions
{
    public static TDestination AdaptIgnoreNull<TSource, TDestination>(this TSource source, TDestination destination)
    {
        return source.Adapt(destination, MapConfig.PatchConfig);
    }
}
