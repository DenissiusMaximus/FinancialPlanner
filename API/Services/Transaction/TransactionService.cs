using API.Domain.BalanceManagement;
using API.Extensions;
using API.Inputs;
using API.Models;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Transaction;

public class TransactionService(AppDbContext context, ICurrentUserContext currentUserContext, NotificationContext notificationContext, IBalanceManagementService balanceManagementService) : ITransactionService
{
    public async Task<TransactionDto?> CreateTransaction(CreateTransactionInput createTransactionInput)
    {
        var userId = currentUserContext.RequiredUserId;

        await using var dbTransaction = await context.Database.BeginTransactionAsync();

        var transaction = createTransactionInput.Adapt<Models.Transaction>();
        transaction.UserId = userId;

        var addedTransaction = context.Transactions.Add(transaction);

        if (addedTransaction == null)
        {
            notificationContext.AddNotification("Failed to create transaction", ErrorType.ServerError);
            return null;
        }

        var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == addedTransaction.Entity.SourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        if (!await balanceManagementService.UpdateAmounts(addedTransaction.Entity, source, userId))
            return null;

        await context.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        return await context.Transactions
        .AsNoTracking()
        .ProjectToType<TransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == transaction.Id && t.UserId == userId);

    }

    public async Task<bool> DeleteTransaction(int id)
    {
        var userId = currentUserContext.RequiredUserId;
        var transaction = context.Transactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return false;
        }

        var dbTransaction = await context.Database.BeginTransactionAsync();

        if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Adjustment)
        {
            notificationContext.AddNotification("Adjustment transactions cannot be deleted", ErrorType.BadRequest);
            return false;
        }

        if (!await balanceManagementService.ResetTransaction(transaction, userId))
            return false;

        context.Transactions.Remove(transaction);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<TransactionDto?> UpdateTransaction(int id, UpdateTransactionInput transactionUpdateDto)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = context.Transactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);

        var originalTransaction = transaction.Adapt<Models.Transaction>();

        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return null;
        }

        if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Adjustment)
        {
            notificationContext.AddNotification("Adjustment transactions cannot be updated", ErrorType.BadRequest);
            return null;
        }

        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        var updatedTransaction = transactionUpdateDto.AdaptIgnoreNull(transaction);

        if (updatedTransaction.Amount != originalTransaction.Amount || updatedTransaction.TransactionTypeId != originalTransaction.TransactionTypeId || updatedTransaction.SourceId != originalTransaction.SourceId || updatedTransaction.DestinationSourceId != originalTransaction.DestinationSourceId)
        {
            if (!await balanceManagementService.ResetTransaction(originalTransaction, userId))
                return null;

            var source = await context.Sources.FirstOrDefaultAsync(s => s.Id == updatedTransaction.SourceId && s.UserId == userId);
            if (source == null)
            {
                notificationContext.AddNotification("Source not found", ErrorType.NotFound);
                return null;
            }

            if (!await balanceManagementService.UpdateAmounts(updatedTransaction, source, userId))
                return null;

        }

        await context.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        var updatedTransactionDto = await context.Transactions
        .AsNoTracking()
        .ProjectToType<TransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        return updatedTransactionDto;

    }

    public async Task<TransactionDto?> GetTransactionById(int id)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = await context.Transactions
        .AsNoTracking()
        .ProjectToType<TransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return null;
        }

        return transaction;
    }

    public async Task<IReadOnlyCollection<TransactionDto>> GetUsersTransactions(GetUserTransactionsInput input)
    {
        var userId = currentUserContext.RequiredUserId;

        var transactions = context.Transactions
        .AsNoTracking()
        .ProjectToType<TransactionDto>()
        .Where(t => t.UserId == userId);

        if (input.CategoryId != null)
            transactions = transactions.Where(t => t.Category != null && t.Category.Id == input.CategoryId);

        if (input.FromDate != null)
            transactions = transactions.Where(t => t.Date >= input.FromDate);

        if (input.ToDate != null)
            transactions = transactions.Where(t => t.Date <= input.ToDate);

        if (input.SortBy != null)
        {
            transactions = input.SortBy switch
            {
                TransactionSortBy.Date =>
                input.SortDescending ? transactions.OrderByDescending(t => t.Date) : transactions.OrderBy(t => t.Date),

                TransactionSortBy.Amount => input.SortDescending ? transactions.OrderByDescending(t => t.Amount) : transactions.OrderBy(t => t.Amount),

                _ => transactions
            };
        }

        return await transactions
        .Skip(input.Offset)
        .Take(input.Limit)
        .ToListAsync();
    }
}
