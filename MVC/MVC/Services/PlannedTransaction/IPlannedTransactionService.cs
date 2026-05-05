using API.Dtos;
using API.Models;

namespace API.Services.PlannedTransaction;

public interface IPlannedTransactionService
{
    Task<PaginatedResult<PlannedTransactionDto>> GetUsersPlannedTransactions(GetUserPlannedTransactionsInput input);
    Task<PlannedTransactionDto?> GetPlannedTransactionById(int id);
    Task<PlannedTransactionDto?> CreatePlannedTransaction(CreatePlannedTransactionInput input);
    Task<PlannedTransactionDto?> UpdatePlannedTransaction(int id, UpdatePlannedTransactionInput transactionUpdateDto);
    Task<bool> DeletePlannedTransaction(int id);
}
