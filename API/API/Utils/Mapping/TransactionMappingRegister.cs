using API.Inputs;
using API.Models;
using Mapster;

namespace API.Extensions;

public class TransactionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateTransactionInput, Models.Transaction>()
            .AfterMapping((src, dest) =>
            {
                if (src.CategoryId == 0 || src.CategoryId == -1)
                    dest.CategoryId = null;

                if (src.DestinationSourceId == 0 || src.DestinationSourceId == -1)
                    dest.DestinationSourceId = null;
            });


        config.NewConfig<Transaction, Transaction>()
            .Ignore(dest => dest.Category!)
            .Ignore(dest => dest.Currency)
            .Ignore(dest => dest.DestinationSource!)
            .Ignore(dest => dest.Source)
            .Ignore(dest => dest.TransactionType)
            .Ignore(dest => dest.User);


        config.NewConfig<CreateTransactionInput, Transaction>()
            .AfterMapping((src, dest) =>
            {
                if (src.CategoryId == 0 || src.CategoryId == -1)
                    dest.CategoryId = null;

                if (src.DestinationSourceId == 0 || src.DestinationSourceId == -1)
                    dest.DestinationSourceId = null;
            });
    }
}
