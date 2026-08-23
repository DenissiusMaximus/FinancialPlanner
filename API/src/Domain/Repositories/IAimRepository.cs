using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Domain.Repositories;

public interface IAimRepository
{
    Task<Aim?> GetByIdAsync(int id, int userId, CancellationToken ct);

    Task<IReadOnlyList<Aim>> GetAllAsync(int userId, CancellationToken ct);

    Task<Aim> AddAsync(Aim aim, CancellationToken ct);

    void Remove(Aim aim);

    Task<SourceAim?> GetSourceLinkAsync(int aimId, int sourceId, CancellationToken ct);

    void AddSourceLink(SourceAim sourceAim);

    void RemoveSourceLink(SourceAim sourceAim);
}
