using API.Domain.BalanceManagement;
using API.Dtos;
using API.Extensions;
using API.Inputs;
using API.Models;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Transaction;

public class TransactionService(
    AppDbContext context,
    ICurrentUserContext currentUserContext,
    NotificationContext notificationContext,
    IBalanceManagementService balanceManagementService) : ITransactionService
{
    public async Task<TransactionDto?> CreateTransaction(CreateTransactionInput createTransactionInput)
    {
        var userId = currentUserContext.RequiredUserId;

        var source =
            await context.Sources.FirstOrDefaultAsync(s =>
                s.Id == createTransactionInput.SourceId && s.UserId == userId);

        if (source == null)
        {
            notificationContext.AddNotification("Source not found", ErrorType.NotFound);
            return null;
        }

        var transaction = createTransactionInput.Adapt<Models.Transaction>();
        transaction.UserId = userId;

        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        
        var addedTransaction = context.Transactions.Add(transaction);

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

        var result = await context.SaveChangesAsync();
        await dbTransaction.CommitAsync();
        
        return result > 0;
    }

    public async Task<TransactionDto?> UpdateTransaction(int id, UpdateTransactionInput transactionUpdateDto)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = context.Transactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return null;
        }

        var originalTransaction = transaction.Adapt<Models.Transaction>();

        if (transaction.TransactionTypeId == (int)TransactionTypeEnum.Adjustment)
        {
            notificationContext.AddNotification("Adjustment transactions cannot be updated", ErrorType.BadRequest);
            return null;
        }

        var updatedTransaction = transactionUpdateDto.AdaptIgnoreNull(transaction);
        
        await using var dbTransaction = await context.Database.BeginTransactionAsync();

        if (updatedTransaction.Amount != originalTransaction.Amount ||
            updatedTransaction.TransactionTypeId != originalTransaction.TransactionTypeId ||
            updatedTransaction.SourceId != originalTransaction.SourceId || updatedTransaction.DestinationSourceId !=
            originalTransaction.DestinationSourceId)
        {
            if (!await balanceManagementService.ResetTransaction(originalTransaction, userId))
                return null;

            var source =
                await context.Sources.FirstOrDefaultAsync(s =>
                    s.Id == updatedTransaction.SourceId && s.UserId == userId);
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

    public async Task<PaginatedResult<TransactionDto>> GetUsersTransactions(GetUserTransactionsInput input)
    {
        var userId = currentUserContext.RequiredUserId;

        var transactions = await context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .FilterByCategory(input.CategoryId)
            .FilterByDateRange(input.FromDate, input.ToDate)
            .ApplySorting(input.SortBy, input.SortDescending)
            .ProjectToType<TransactionDto>()
            .Skip(input.Offset)
            .Take(input.Limit)
            .ToListAsync();
        
        var totalCount = await context.Transactions.CountAsync(t => t.UserId == userId);

        var paginated = new PaginatedResult<TransactionDto>
        {
            Data = transactions,
            Meta = new PaginationMeta
            {
                TotalCount = totalCount,
                Limit = input.Limit,
                Offset = input.Offset
            }
        };

        return paginated;
    }
}