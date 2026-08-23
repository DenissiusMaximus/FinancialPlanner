using FinancialPlanner.Application.Features.Sources.Dtos;
using FinancialPlanner.Domain.Entities;
using Mapster;

namespace FinancialPlanner.Application.Features.Sources;

public sealed class SourceMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Source, SourceDtoDetailed>()
            .Map(dest => dest.Aims, src => src.SourceAims.Select(sa => sa.Aim));
    }
}
