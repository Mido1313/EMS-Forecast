namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class LocationTypeMapping
{
    public static void Map(this EntityTypeBuilder<LocationType> builder)
    {
        builder.ToTable("LocationTypes");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.LocationTypeName).AsRequiredText(256);
        builder.HasIndex(l => l.LocationTypeName).IsUnique();
    }
}
