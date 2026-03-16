using API.Inputs;
using API.Models;
using API.Utils.Map;
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
        .Include(t => t.Source)
        .Include(t => t.DestinationSource)
        .Include(t => t.Category)
        .Include(t => t.Currency)
        .Include(t => t.TransactionType)
        .FirstOrDefaultAsync(t => t.Id == addedTransaction.Entity.Id && t.UserId == userId);

        return createdTransaction.Adapt<TransactionDto>();
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

        var transaction = await context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction not found", ErrorType.NotFound);
            return null;
        }

        return transaction.Adapt<TransactionDto>();
    }

    public async Task<IReadOnlyCollection<TransactionDto>> GetUsersTransactions()
    {
        var userId = currentUserContext.RequiredUserId;

        var transactions = await context.Transactions.AsNoTracking().Where(t => t.UserId == userId).Select(t => t.Adapt<TransactionDto>()).ToListAsync();

        return transactions;
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
        .Include(t => t.Source)
        .Include(t => t.DestinationSource)
        .Include(t => t.Category)
        .Include(t => t.Currency)
        .Include(t => t.TransactionType)
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        return updatedTransaction.Adapt<TransactionDto>();
    }
}
