using Base.Tools;

using Core.Entities;

using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace Persistence;

using Persistence.Mapping;

public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Parameterless constructor reads the connection string from appsettings.json (at design time)
    /// For migration generation! Note: The constructor must be the first one in order.
    /// </summary>
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            //We need this for migration
            var connectionString = ConfigurationHelper.GetConfiguration().Get("DefaultConnection", "ConnectionStrings");
            optionsBuilder.UseNpgsql(connectionString);
        }

        optionsBuilder.LogTo(message => Debug.WriteLine(message));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<District>().Map();
        modelBuilder.Entity<PostalCode>().Map();
        modelBuilder.Entity<Municipality>().Map();
        modelBuilder.Entity<Population>().Map();
        modelBuilder.Entity<LocationType>().Map();
        modelBuilder.Entity<IncidentType>().Map();
        modelBuilder.Entity<Incident>().Map();
        modelBuilder.Entity<Attraction>().Map();
        modelBuilder.Entity<NursingHome>().Map();
        modelBuilder.Entity<Event>().Map();
        modelBuilder.Entity<Weather>().Map();
        modelBuilder.Entity<PublicHoliday>().Map();
        modelBuilder.Entity<TrafficHotspot>().Map();
        modelBuilder.Entity<Traffic>().Map();
        modelBuilder.Entity<TrafficAccident>().Map();
        modelBuilder.Entity<TrafficConstruction>().Map();
        modelBuilder.Entity<AccidentHistory>().Map();
        modelBuilder.Entity<Result>().Map();
    }
}
