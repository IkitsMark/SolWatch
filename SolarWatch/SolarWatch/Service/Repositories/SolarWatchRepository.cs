using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarWatch.Data;
using SolarWatch.Model;

namespace SolarWatch.Service.Repositories;

public class SolarWatchRepository : ISolarWatchRepository
{
    private SolarWatchContext _dbContext;
    private readonly ILogger<SolarWatchRepository> _logger;

    public SolarWatchRepository(SolarWatchContext context, ILogger<SolarWatchRepository> logger)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<SolarData?> GetSolarWatchAsync(int cityId, DateTime? date, string timeZone)
    {
        try
        {
            date ??= DateTime.UtcNow.Date;

            var startOfPrevDay = date.Value.Date.AddDays(-1);
            var endOfNextDay = date.Value.Date.AddDays(2);

            return await _dbContext.SolarDatas
                .AsNoTracking()
                .FirstOrDefaultAsync(data =>
                data.Sunrise.Date >= startOfPrevDay &&
                data.Sunset.Date <= endOfNextDay &&
                data.CityId == cityId &&
                data.TimeZone == timeZone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching solar data for CityId: {CityId} and TimeZone: {TimeZone}", cityId, timeZone);
            throw;
        }
    }

    public async Task<IReadOnlyList<SolarData>> GetAllSolarWatchAsync()
    {
        try
        {
            return await _dbContext.SolarDatas
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all solar data.");
            throw;
        }
    }

    public async Task AddAsync(SolarData data)
    {
        try
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _logger.LogInformation("Adding new SolarData for CityId: {CityId} on {Date}", data.CityId, data.SearchDate);
            await _dbContext.AddAsync(data);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding solar data.");
            throw;
        }
    }

    public async Task UpdateAsync(SolarData newData)
    {
        try
        {
            if (newData == null) throw new ArgumentNullException(nameof(newData));

            var dbRepresentation = await _dbContext.SolarDatas.FindAsync(newData.Id);
            if (dbRepresentation == null)
            {
                _logger.LogWarning("Attempted to update non-existent SolarData with ID: {Id}", newData.Id);
                throw new InvalidOperationException($"SolarData with ID {newData.Id} not found.");
            }

            dbRepresentation.Sunrise = newData.Sunrise;
            dbRepresentation.Sunset = newData.Sunset;
            dbRepresentation.CityId = newData.CityId;
            dbRepresentation.TimeZone = newData.TimeZone;
            dbRepresentation.SearchDate = newData.SearchDate;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated SolarData with ID: {Id}", newData.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating solar data with ID: {Id}", newData.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var dbRepresentation = await _dbContext.SolarDatas.FindAsync(id);
            if (dbRepresentation == null)
            {
                _logger.LogWarning("Attempted to delete non-existent SolarData with ID: {Id}", id);
                throw new InvalidOperationException($"SolarData with ID {id} not found.");
            }

            _dbContext.SolarDatas.Remove(dbRepresentation);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Deleted SolarData with ID: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting solar data with ID: {Id}", id);
            throw;
        }
    }
}