using API.Inputs;
using API.Models;

namespace API.Services;

public interface ITransactionService
{
    Task<IReadOnlyCollection<TransactionDto>> GetUsersTransactions(int userId);
    Task<TransactionDto> GetTransactionById(int id, int userId);
    Task<TransactionDto> CreateTransaction(TransactionInput transactionCreateDto, int userId);
    Task<TransactionDto> UpdateTransaction(int id, TransactionInput transactionUpdateDto, int userId);
    Task<bool> DeleteTransaction(int id, int userId);
}
