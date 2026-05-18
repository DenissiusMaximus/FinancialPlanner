using API.Services.Transaction;

namespace API.Domain.BalanceManagement.Strategies
{
    /// <summary>
    /// Defines a strategy for calculating balance modifications based on the transaction type.
    /// </summary>
    public interface IBalanceCalculationStrategy
    {
        /// <summary>
        /// Checks if the current strategy applies to the provided transaction type.
        /// </summary>
        bool AppliesTo(TransactionTypeEnum type);

        /// <summary>
        /// Calculates the new balance after applying the transaction amount.
        /// </summary>
        decimal CalculateNewBalance(decimal currentBalance, decimal transactionAmount);
    }
}