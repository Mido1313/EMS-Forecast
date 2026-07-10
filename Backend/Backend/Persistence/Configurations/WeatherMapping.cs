namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class WeatherMapping
{
    public static void Map(this EntityTypeBuilder<Weather> builder)
    {
        builder.ToTable("Weathers");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Temperature).AsDecimal(6, 2);
        builder.Property(w => w.Precipitation).AsDecimal(6, 2);
        builder.Property(w => w.Snow).AsDecimal(6, 2);
        builder.Property(w => w.WindSpeed).AsDecimal(6, 2);
        builder.Property(w => w.AirQuality).AsDecimal(6, 2);
        builder.Property(w => w.PollenTotal).AsDecimal(6, 2);
        builder.Property(w => w.PollenBirch).AsDecimal(6, 2);
        builder.Property(w => w.PollenGrass).AsDecimal(6, 2);
        builder.Property(w => w.ParticulateMatter).AsDecimal(6, 2);
        builder.Property(w => w.Visibility).AsDecimal(6, 2);

        builder.HasOne(w => w.PostalCode)
              .WithMany(p => p.Weathers)
              .HasForeignKey(w => w.PostalCodeId)
              .HasPrincipalKey(p => p.Plz)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
