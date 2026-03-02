using API.Dtos;

namespace API.Services.Source;

public interface ISourceService
{
    Task<SourceDto?> CreateSource(CreateSourceInput createSourceDto, int userId);
    Task<bool> DeleteSource(int sourceId, int userId);
    Task<SourceDto?> GetSourceById(int sourceId, int userId);
    Task<List<SourceDto>> GetSources(int userId);
    Task<SourceDto?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto, int userId);
}
