// 

using Microsoft.EntityFrameworkCore;
using SolarWatch.Model;

namespace SolarWatch.Data;

public class SolarWatchContext : DbContext
{
    public DbSet<City> Cities { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=WeatherApi;User Id=sa;Password=yourStrong(!)Password;Encrypt=False;");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        /*builder.Entity<SunriseSunset>().HasIndex(s => s.name);*/
    }
}