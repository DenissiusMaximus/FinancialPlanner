using API.Models;
using Mapster;

namespace API.Extensions;

public class PlannedTransactionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreatePlannedTransactionInput, Models.PlannedTransaction>()
            .AfterMapping((src, dest) =>
            {
                if (src.CategoryId == 0 || src.CategoryId == -1)
                    dest.CategoryId = null;
            });

        config.NewConfig<UpdatePlannedTransactionInput, Models.PlannedTransaction>()
            .AfterMapping((src, dest) =>
            {
                if (src.CategoryId == 0 || src.CategoryId == -1)
                    dest.CategoryId = null;
            });
    }
}
