using System;
using API.Models;

namespace API.Services.Frequency;

public interface IFrequencyService
{
    public Task<List<FrequencyDto>> GetFrequencies(int userId);
    public Task<List<FrequencyDto>> GetUserFrequencies(int userId);
    public Task<FrequencyDto?> GetFrequency(int id, int userId);
    public Task<FrequencyDto?> CreateFrequency(FrequencyInput frequency, int userId);
    Task<FrequencyDto?> UpdateFrequency(FrequencyInput frequency, int id, int userId);
    public Task<bool> DeleteFrequency(int id, int userId);
}