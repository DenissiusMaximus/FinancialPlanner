using FinancialPlanner.Application.Features.Aims.Dtos;
using FinancialPlanner.Domain.Entities;
using Mapster;

namespace FinancialPlanner.Application.Common.Mapping;

public sealed class AimMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Aim, AimDto>()
            .Map(dest => dest.Sources, src => src.SourceAims.Select(sa => sa.Source));
    }
}
