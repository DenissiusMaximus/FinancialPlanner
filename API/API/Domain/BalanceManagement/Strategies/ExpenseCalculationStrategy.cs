using API.Services.Transaction;

namespace API.Domain.BalanceManagement.Strategies
{
    /// <summary>
    /// Strategy for handling expense transactions.
    /// Decreases the account balance.
    /// </summary>
    public class ExpenseCalculationStrategy : IBalanceCalculationStrategy
    {
        public bool AppliesTo(TransactionTypeEnum type) => type == TransactionTypeEnum.Expense;

        public decimal CalculateNewBalance(decimal currentBalance, decimal transactionAmount)
        {
            return currentBalance - transactionAmount;
        }
    }
}