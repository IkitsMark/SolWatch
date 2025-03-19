using SolarWatch.Model;

namespace SolarWatch.Service.Repositories;

public interface ISolarWatchRepository
{
    public Task<SolarData?> GetSolarWatchAsync(int cityId, DateTime? date, string TimeZone);
    public Task<IReadOnlyList<SolarData>> GetAllSolarWatchAsync();
    public Task AddAsync(SolarData data);
    public Task UpdateAsync(SolarData newData);
    public Task DeleteAsync(int id);
}