using API.Inputs;
using API.Models;
using Mapster;

namespace API.Extensions;

public class AimMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Aim, AimDto>()
            .Map(dest => dest.Sources, src => src.SourceAims.Select(sa => sa.Source));
    }
}
