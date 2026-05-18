using API.Services.Transaction;

namespace API.Domain.BalanceManagement.Strategies
{
    /// <summary>
    /// Strategy for handling income transactions.
    /// Increases the account balance.
    /// </summary>
    public class IncomeCalculationStrategy : IBalanceCalculationStrategy
    {
        public bool AppliesTo(TransactionTypeEnum type) => type == TransactionTypeEnum.Income;

        public decimal CalculateNewBalance(decimal currentBalance, decimal transactionAmount)
        {
            return currentBalance + transactionAmount;
        }
    }
}