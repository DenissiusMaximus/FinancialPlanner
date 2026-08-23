namespace FinancialPlanner.Application.Abstractions;

public interface IPatchMapper
{
    TDestination PatchInto<TSource, TDestination>(TSource source, TDestination destination);
}
