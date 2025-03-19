using SolarWatch.Model;

namespace SolarWatch.Service.Repositories;

public interface ICityRepository
{
    public Task<IReadOnlyList<City>> GetAllCitiesAsync();
    public Task<City?> GetByNameAsync(string name);
    public Task<City?> GetByIdAsync(int id);
    public Task AddAsync(City city);
    public Task UpdateAsync(City city);
    public Task DeleteAsync(int id);
}