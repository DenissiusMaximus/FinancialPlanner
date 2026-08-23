using FinancialPlanner.Application.Abstractions;
using Mapster;

namespace FinancialPlanner.Application.Common.Mapping;

public sealed class PatchMapper : IPatchMapper
{
    public TDestination PatchInto<TSource, TDestination>(TSource source, TDestination destination)
        => source.Adapt(destination, PatchMapperConfig.Instance);
}
