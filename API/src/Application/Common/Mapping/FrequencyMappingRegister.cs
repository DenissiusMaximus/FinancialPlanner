using FinancialPlanner.Application.Common.Dtos;
using FinancialPlanner.Domain.Entities;
using Mapster;

namespace FinancialPlanner.Application.Common.Mapping;

public sealed class FrequencyMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Frequency, FrequencyDto>()
            .Map(dest => dest.IntervalUnit, src => src.IntervalUnitNavigation);
    }
}
