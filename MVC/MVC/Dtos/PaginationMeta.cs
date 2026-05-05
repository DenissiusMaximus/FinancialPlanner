namespace API.Dtos;

public class PaginationMeta
{
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}