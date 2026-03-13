using API.Dtos;

namespace API.Services.Source;

public interface ISourceService
{
    Task<SourceDto?> CreateSource(CreateSourceInput createSourceDto);
    Task<SourceDto?> ArchiveSource(int sourceId);
    Task<SourceDto?> UnArchiveSource(int sourceId);
    Task<SourceDto?> GetSourceById(int sourceId);
    Task<IReadOnlyCollection<SourceDto>> GetSources();
    Task<SourceDto?> UpdateSource(int sourceId, UpdateSourceInput updateSourceDto);
}
