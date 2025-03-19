using Microsoft.EntityFrameworkCore;
using SolarWatch.Data;
using SolarWatch.Model;

namespace SolarWatch.Service.Repositories;

public class CityRepository : ICityRepository
{
    private readonly SolarWatchContext _dbContext;
    private readonly ILogger<CityRepository> _logger;

    public CityRepository(SolarWatchContext dbcontext, ILogger<CityRepository> logger)
    {
        _dbContext = dbcontext;
        _logger = logger;
    }
    //retrive all cities from the database, log error on failiure
    public async Task<IReadOnlyList<City>> GetAllCitiesAsync()
    {
        try
        {
            return await _dbContext.Cities.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all cities from the database.");
            throw;
        }
    }
    //retrive a city by name, log error on failiure
    public async Task<City?> GetByNameAsync(string name)
    {
        try
        {
            return await _dbContext.Cities.FirstOrDefaultAsync(c => c.Name == name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving city by name: {CityName}", name);
            throw;
        }
    }
    //retrive a city by ID, log error on failiure
    public async Task<City?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Cities.FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving city by ID: {CityId}", id);
            throw;
        }
    }
    //add a city to the database async, check for duplicate before adding
    //log error on duplicate found
    //log info when new city is added
    //log error if operation fails
    public async Task AddAsync(City city)
    {
        try
        {
            var existingCity = await _dbContext.Cities.AnyAsync(c => c.Name == city.Name);
            if (existingCity)
            {
                _logger.LogWarning("Attempted to add duplicate city: {CityName}", city.Name);
                return;
            }

            _logger.LogInformation("Adding new city to database: {CityName}", city.Name);
            await _dbContext.AddAsync(city);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding city: {CityName}", city.Name);
            throw;
        }
    }
    //updates an existing city in the database async
    //checks if the city exists before updating
    //logs a warning if the city does not exist.
    //logs info when city is updated
    //logs error if operation fails
    public async Task UpdateAsync(City city)
    {
        try
        {
            var cityDbRepresentation = await _dbContext.Cities.FirstOrDefaultAsync(c => c.Id == city.Id);

            if (cityDbRepresentation == null)
            {
                _logger.LogWarning("Attempted to update non-existent city with ID: {CityId}", city.Id);
                throw new InvalidOperationException($"City with ID {city.Id} not found.");
            }

            cityDbRepresentation.Name = city.Name;
            cityDbRepresentation.Country = city.Country;
            cityDbRepresentation.State = city.State;
            cityDbRepresentation.Latitude = city.Latitude;
            cityDbRepresentation.Longitude = city.Longitude;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated city: {CityName}", city.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating city: {CityName}", city.Name);
            throw;
        }
    }
    //deletes a city by ID from the database async
    //checks if the city exists before deleting
    //logs a warning if the city does not exist.
    //logs info when a city is deleted
    //logs error if operation fails
    public async Task DeleteAsync(int id)
    {
        try
        {
            var city = await _dbContext.Cities.FindAsync(id);
            if (city == null)
            {
                _logger.LogWarning("Attempted to delete non-existent city with ID: {CityId}", id);
                throw new InvalidOperationException($"City with ID {id} not found.");
            }

            _dbContext.Cities.Remove(city);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Deleted city with ID: {CityId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting city with ID: {CityId}", id);
            throw;
        }
    }
}