using System;
using API.Models;

namespace API.Services.PlannedTransaction;

public interface IPlannedTransactionService
{
    Task<IReadOnlyCollection<PlannedTransactionDto>> GetUsersPlannedTransactions(int limit = 100, int offset = 0);
    Task<PlannedTransactionDto?> GetPlannedTransactionById(int id);
    Task<PlannedTransactionDto?> CreatePlannedTransaction(CreatePlannedTransactionInput input);
    Task<PlannedTransactionDto?> UpdatePlannedTransaction(int id, UpdatePlannedTransactionInput transactionUpdateDto);
    Task<bool> DeletePlannedTransaction(int id);
}
