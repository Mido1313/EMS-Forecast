namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class PostalCodeMapping
{
    public static void Map(this EntityTypeBuilder<PostalCode> builder)
    {
        builder.ToTable("PostalCodes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Plz).AsRequiredText(10);
        builder.Property(p => p.CityName).AsRequiredText(256);

        builder.HasIndex(p => p.Plz).IsUnique();

        builder.HasOne(p => p.District)
              .WithMany(d => d.PostalCodes)
              .HasForeignKey(p => p.DistrictId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
