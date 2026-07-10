namespace Persistence.Mapping;

using Base.Persistence.Mappings;

using Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class DistrictMapping
{
    public static void Map(this EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DistrictName).AsRequiredText(256);
        builder.HasIndex(d => d.DistrictName).IsUnique();
        builder.HasIndex(d => d.DistrictId).IsUnique();
    }
}