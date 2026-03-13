using System;
using API.Models;

namespace API.Services.Frequency;

public interface IFrequencyService
{
    Task<IReadOnlyCollection<FrequencyDto>> GetFrequencies(int userId);
    Task<IReadOnlyCollection<FrequencyDto>> GetUserFrequencies(int userId);
    Task<FrequencyDto?> GetFrequency(int id, int userId);
    Task<FrequencyDto?> CreateFrequency(FrequencyInput frequency, int userId);
    Task<FrequencyDto?> UpdateFrequency(FrequencyInput frequency, int id, int userId);
    Task<bool> DeleteFrequency(int id, int userId);
}