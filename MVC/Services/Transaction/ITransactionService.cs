using API.Inputs;
using API.Models;

namespace API.Services;

public interface ITransactionService
{
    Task<IReadOnlyCollection<TransactionDto>> GetUsersTransactions(int limit = 100, int offset = 0);
    Task<TransactionDto?> GetTransactionById(int id);
    Task<TransactionDto?> CreateTransaction(CreateTransactionInput transactionCreateDto);
    Task<TransactionDto?> UpdateTransaction(int id, UpdateTransactionInput transactionUpdateDto);
    Task<bool> DeleteTransaction(int id);
}
