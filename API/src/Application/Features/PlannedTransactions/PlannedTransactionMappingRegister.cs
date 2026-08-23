using FinancialPlanner.Application.Features.PlannedTransactions.Commands.CreatePlannedTransaction;
using FinancialPlanner.Application.Features.PlannedTransactions.Commands.UpdatePlannedTransaction;
using FinancialPlanner.Domain.Entities;
using Mapster;

namespace FinancialPlanner.Application.Features.PlannedTransactions;

public sealed class PlannedTransactionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreatePlannedTransactionCommand, PlannedTransaction>()
            .AfterMapping((_, dest) => NormalizeCategoryId(dest));

        config.NewConfig<UpdatePlannedTransactionCommand, PlannedTransaction>()
            .AfterMapping((_, dest) => NormalizeCategoryId(dest));
    }

    private static void NormalizeCategoryId(PlannedTransaction dest)
    {
        if (dest.CategoryId is 0 or -1)
            dest.CategoryId = null;
    }
}
