using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SolarWatch.Model;

namespace SolarWatch.Data;

public class SolarWatchContext : DbContext
{
    public DbSet<City> Cities { get; set; }
    public DbSet<SolarData> SolarDatas { get; set; }

    //constructor for Dependency Injenction!
    public SolarWatchContext(DbContextOptions<SolarWatchContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<City>()
            .HasIndex(u => u.Name)
            .IsUnique(); // Ensures City names are unique, but there could be cities with the same name.

        builder.Entity<City>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Entity<SolarData>()
            .Property(sd => sd.Id)
            .ValueGeneratedOnAdd();
    }
}