// 

using SolarWatch.Data;
using SolarWatch.Model;

namespace SolarWatch.Service.Repositories;

public class CityRepository : ICityRepository
{
    private readonly SolarWatchContext _dbContext;

    public CityRepository(SolarWatchContext dbcontext)
    {
        _dbContext = dbcontext;
    }
    public IEnumerable<City> GetAll()
    {
        return _dbContext.Cities.ToList();
    }

    public City? GetByName(string name)
    {
        return _dbContext.Cities.FirstOrDefault(c => c.Name == name);
    }

    public void Add(City city)
    {
        _dbContext.Add(city);
        _dbContext.SaveChanges();
    }

    public void Delete(City city)
    {
        _dbContext.Remove(city);
        _dbContext.SaveChanges();
    }

    public void Update(City city)
    {  
        _dbContext.Update(city);
        _dbContext.SaveChanges();
    }
}