using API.Extensions;
using API.Models;
using API.Utils.Notification;
using API.Utils.UserContext;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace API.Services.PlannedTransaction;

public class PlannedTransactionService(AppDbContext context, ICurrentUserContext currentUserContext, NotificationContext notificationContext) : IPlannedTransactionService
{
    public async Task<PlannedTransactionDto?> CreatePlannedTransaction(CreatePlannedTransactionInput input)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = input.Adapt<Models.PlannedTransaction>();
        transaction.UserId = userId;

        var addedTransaction = context.PlannedTransactions.Add(transaction);

        await context.SaveChangesAsync();

        var createdTransaction = await context.PlannedTransactions
        .AsNoTracking()
        .ProjectToType<PlannedTransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == addedTransaction.Entity.Id && t.UserId == userId);

        return createdTransaction;
    }

    public async Task<bool> DeletePlannedTransaction(int id)
    {
        var userId = currentUserContext.RequiredUserId;
        var transaction = context.PlannedTransactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Planned transaction not found", ErrorType.NotFound);
            return false;
        }

        context.PlannedTransactions.Remove(transaction);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<PlannedTransactionDto?> GetPlannedTransactionById(int id)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = await context.PlannedTransactions
        .AsNoTracking()
        .ProjectToType<PlannedTransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Planned transaction not found", ErrorType.NotFound);
            return null;
        }

        return transaction;
    }

    public async Task<IReadOnlyCollection<PlannedTransactionDto>> GetUsersPlannedTransactions(int limit = 100, int offset = 0)
    {
        var userId = currentUserContext.RequiredUserId;

        var transactions = await context.PlannedTransactions
        .AsNoTracking()
        .ProjectToType<PlannedTransactionDto>()
        .Where(t => t.UserId == userId)
        .Skip(offset)
        .Take(limit)
        .ToListAsync();

        return transactions;
    }

    public async Task<PlannedTransactionDto?> UpdatePlannedTransaction(int id, UpdatePlannedTransactionInput transactionUpdateDto)
    {
        var userId = currentUserContext.RequiredUserId;

        var transaction = context.PlannedTransactions.FirstOrDefault(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            notificationContext.AddNotification("Planned transaction not found", ErrorType.NotFound);
            return null;
        }

        transactionUpdateDto.AdaptIgnoreNull(transaction);

        await context.SaveChangesAsync();

        var updatedTransaction = await context.PlannedTransactions
        .AsNoTracking()
        .ProjectToType<PlannedTransactionDto>()
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        return updatedTransaction;
    }
}
