using System;
using API.Inputs;
using API.Models;

namespace API.Services.Frequency;

public interface IFrequencyService
{
    Task<IReadOnlyCollection<FrequencyDto>> GetFrequencies();
    Task<IReadOnlyCollection<FrequencyDto>> GetUserFrequencies();
    Task<FrequencyDto?> GetFrequency(int id);
    Task<FrequencyDto?> CreateFrequency(FrequencyInput frequency);
    Task<FrequencyDto?> UpdateFrequency(FrequencyInput frequency, int id);
    Task<bool> DeleteFrequency(int id);
}