using API.Services.Transaction;
using API.Utils.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Collections.Generic;
using API.Domain.BalanceManagement.Strategies;

namespace API.Domain.BalanceManagement
{
    public class BalanceManagementService : IBalanceManagementService
    {
        private readonly IEnumerable<IBalanceCalculationStrategy> _strategies;

        // Injecting all registered strategies
        public BalanceManagementService(IEnumerable<IBalanceCalculationStrategy> strategies)
        {
            _strategies = strategies;
        }

        public void UpdateBalance(Source source, Transaction transaction)
        {
            var strategy = _strategies.FirstOrDefault(s => s.AppliesTo(transaction.Type));
            
            if (strategy == null)
            {
                throw new System.InvalidOperationException($"No balance calculation strategy found for transaction type: {transaction.Type}");
            }

            source.Balance = strategy.CalculateNewBalance(source.Balance, transaction.Amount);
        }
        
    public bool IsTransactionModified(Models.Transaction original, Models.Transaction updated)
    {
        return original.Amount != updated.Amount || 
               original.TransactionTypeId != updated.TransactionTypeId || 
               original.SourceId != updated.SourceId || 
               original.DestinationSourceId != updated.DestinationSourceId;
    }
    
    public async Task<bool> TransferTransaction(int userId, Models.Transaction transaction, Models.Source source)
    {
        var destinationSource = await context.Sources.FirstOrDefaultAsync(s => s.Id == transaction.DestinationSourceId && s.UserId == userId);

        if (destinationSource == null)
        {
            notificationContext.AddNotification("Destination source not found", ErrorType.NotFound);
            return false;
        }

        source.Amount -= transaction.Amount;
        destinationSource.Amount += transaction.Amount;

        return true;
    }

    public async Task<bool> UpdateAmounts(Models.Transaction transaction, Models.Source source, int userId)
    {
        switch (transaction.TransactionTypeId)
        {
            case (int)TransactionTypeEnum.Adjustment:
                source.Amount = transaction.Amount;
                break;
            case (int)TransactionTypeEnum.Expense:
                source.Amount -= transaction.Amount;
                break;
            case (int)TransactionTypeEnum.Income:
                source.Amount += transaction.Amount;
                break;
            case (int)TransactionTypeEnum.Transfer:
                if (!await TransferTransaction(userId, transaction, source))
                    return false;
                break;
        }

        return true;
    }

    public async Task<bool> ResetTransaction(Models.Transaction transaction, int userId)
    {
        if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Transfer)
        {
            var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == transaction.SourceId && s.UserId == userId);

            if (source == null)
            {
                notificationContext.AddNotification("Source not found", ErrorType.NotFound);
                return false;
            }

            var destinationSource = await context.Sources.FirstOrDefaultAsync(s => s.Id == transaction.DestinationSourceId && s.UserId == userId);

            if (destinationSource == null)
            {
                notificationContext.AddNotification("Destination source not found", ErrorType.NotFound);
                return false;
            }

            destinationSource.Amount -= transaction.Amount;
            source.Amount += transaction.Amount;
        }
        else if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Expense)
        {
            var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == transaction.SourceId && s.UserId == userId);

            if (source == null)
            {
                notificationContext.AddNotification("Source not found", ErrorType.NotFound);
                return false;
            }

            source.Amount += transaction.Amount;
        }
        else if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Income)
        {
            var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == transaction.SourceId && s.UserId == userId);

            if (source == null)
            {
                notificationContext.AddNotification("Source not found", ErrorType.NotFound);
                return false;
            }

            source.Amount -= transaction.Amount;
        }
        
        return true;
    }
}
}