using Microsoft.EntityFrameworkCore.Storage;

namespace API.Domain.BalanceManagement;

public interface IBalanceManagementService
{
    Task<bool> TransferTransaction(int userId, Models.Transaction createdTransaction, Models.Source source);
    Task<bool> UpdateAmounts(Models.Transaction createdTransaction, Models.Source source, int userId);
    Task<bool> ResetTransaction(Models.Transaction transaction, int userId);
}
