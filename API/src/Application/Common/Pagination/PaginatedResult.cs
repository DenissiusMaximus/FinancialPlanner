namespace FinancialPlanner.Application.Common.Pagination;

public class PaginatedResult<T>
{
    public IReadOnlyCollection<T> Data { get; set; } = [];

    public PaginationMeta Meta { get; set; } = new();

    public static PaginatedResult<T> Create(IReadOnlyCollection<T> data, int totalCount, int offset, int limit) => new()
    {
        Data = data,
        Meta = new PaginationMeta
        {
            TotalCount = totalCount,
            Offset = offset,
            Limit = limit,
            HasMore = offset + data.Count < totalCount
        }
    };
}
