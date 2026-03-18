using System;
using API.Dtos;
using API.Inputs;
using API.Models;
using Mapster;

namespace API.Extensions;

public static class MapConfig
{
    public static readonly TypeAdapterConfig PatchConfig = new TypeAdapterConfig();

    static MapConfig()
    {
        PatchConfig.Default.IgnoreNullValues(true);

        PatchConfig.NewConfig<UpdateTransactionInput, Transaction>()
            .AfterMapping((src, dest) =>
            {
                if (src.CategoryId == 0 || src.CategoryId == -1)
                    dest.CategoryId = null;

                if (src.DestinationSourceId == 0 || src.DestinationSourceId == -1)
                    dest.DestinationSourceId = null;
            });
        
        TypeAdapterConfig<Frequency, FrequencyDto>.NewConfig()
            .Map(dest => dest.IntervalUnit, src => src.IntervalUnitNavigation.Name);
    }
}

public static class MappingExtensions
{
    public static TDestination AdaptIgnoreNull<TSource, TDestination>(this TSource source, TDestination destination)
    {
        return source.Adapt(destination, MapConfig.PatchConfig);
    }
}
