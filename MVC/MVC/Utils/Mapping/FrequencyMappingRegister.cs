using API.Models;
using Mapster;

namespace API.Extensions;

public class FrequencyMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Frequency, FrequencyDto>()
            .Map(dest => dest.IntervalUnit, src => src.IntervalUnitNavigation);
    }
}
