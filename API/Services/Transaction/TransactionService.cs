using API.Extensions;
using API.Inputs;
using API.Models;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class TransactionService(AppDbContext context, ICurrentUserContext currentUserContext, NotificationContext notificationContext) : ITransactionService
{
    public async Task<TransactionDto?> CreateTransaction(CreateTransactionInput transactionCreateDto)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = transactionCreateDto.Adapt<Models.Transaction>();
        transaction.UserId = userId;

        var addedTransaction = context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        var createdTransaction = await context.Transactions
        .AsNoTracking()
        .ProjectToType<TransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == addedTransaction.Entity.Id && t.UserId == userId);

        return createdTransaction;
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

        context.Transactions.Remove(transaction);
        return await context.SaveChangesAsync() > 0;
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
        .Where(t => t.UserId == userId)
        .Skip(input.Offset)
        .Take(input.Limit);

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

        return await transactions.ToListAsync(); ;
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

        transactionUpdateDto.AdaptIgnoreNull(transaction);

        await context.SaveChangesAsync();

        var updatedTransaction = await context.Transactions
        .AsNoTracking()
        .ProjectToType<TransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        return updatedTransaction;
    }
}
