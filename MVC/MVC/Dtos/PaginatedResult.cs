namespace API.Dtos;

public class PaginatedResult<T>
{
    public IReadOnlyCollection<T> Data { get; set; } = null!;
    public PaginationMeta Meta { get; set; } = null!;
}