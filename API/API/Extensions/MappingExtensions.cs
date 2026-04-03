using Mapster;

namespace API.Extensions;

public static class MappingExtensions
{
    public static TDestination AdaptIgnoreNull<TSource, TDestination>(this TSource source, TDestination destination)
    {
        return source.Adapt(destination, MapConfig.PatchConfig);
    }
}
