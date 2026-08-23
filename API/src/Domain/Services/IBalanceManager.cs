using FinancialPlanner.Domain.Common;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Services;

public interface IBalanceManager
{
    Result Apply(Transaction transaction, Source source, Source? destinationSource);

    Result Revert(Transaction transaction, Source source, Source? destinationSource);

    bool IsBalanceAffected(Transaction original, Transaction updated);
}
