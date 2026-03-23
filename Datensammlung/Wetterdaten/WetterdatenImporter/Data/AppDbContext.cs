using Microsoft.EntityFrameworkCore;
using WetterdatenImporter.Domain.Entities;

namespace WetterdatenImporter.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<MeasurementPoint> MeasurementPoints => Set<MeasurementPoint>();
    public DbSet<WeatherObservedDaily> WeatherObservedDailies => Set<WeatherObservedDaily>();
    public DbSet<WeatherForecastDaily> WeatherForecastDailies => Set<WeatherForecastDaily>();
    public DbSet<EnvironmentDaily> EnvironmentDailies => Set<EnvironmentDaily>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MeasurementPoint>(entity =>
        {
            entity.ToTable("MeasurementPoint");
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.GebietId)
                .IsUnique();

            entity.Property(x => x.Name)
                .HasMaxLength(200);
        });

        modelBuilder.Entity<WeatherObservedDaily>(entity =>
        {
            entity.ToTable("WeatherObservedDaily");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Source)
                .HasMaxLength(100);

            entity.HasIndex(x => new { x.GebietId, x.Date, x.Source })
                .IsUnique();
        });

        modelBuilder.Entity<WeatherForecastDaily>(entity =>
        {
            entity.ToTable("WeatherForecastDaily");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Source)
                .HasMaxLength(100);

            entity.HasIndex(x => new { x.GebietId, x.ForecastDate, x.Source, x.ForecastRunAt })
                .IsUnique();
        });

        modelBuilder.Entity<EnvironmentDaily>(entity =>
        {
            entity.ToTable("EnvironmentDaily");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Source)
                .HasMaxLength(100);

            entity.HasIndex(x => new { x.GebietId, x.Date, x.Source })
                .IsUnique();
        });
    }
}
