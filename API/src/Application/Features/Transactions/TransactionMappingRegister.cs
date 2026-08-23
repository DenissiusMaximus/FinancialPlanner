using FinancialPlanner.Application.Features.Transactions.Commands.CreateTransaction;
using FinancialPlanner.Application.Features.Transactions.Commands.UpdateTransaction;
using FinancialPlanner.Domain.Entities;
using Mapster;

namespace FinancialPlanner.Application.Features.Transactions;

public sealed class TransactionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateTransactionCommand, Transaction>()
            .AfterMapping((_, dest) => Normalize(dest));

        config.NewConfig<UpdateTransactionCommand, Transaction>()
            .Map(dest => dest.Date, src => src.Date!.Value.ToDateTime(TimeOnly.MinValue), srcCond => srcCond.Date.HasValue)
            .AfterMapping((_, dest) => Normalize(dest));
    }

    private static void Normalize(Transaction dest)
    {
        if (dest.CategoryId is 0 or -1)
            dest.CategoryId = null;

        if (dest.DestinationSourceId is 0 or -1)
            dest.DestinationSourceId = null;
    }
}
