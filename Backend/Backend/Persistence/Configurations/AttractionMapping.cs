namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class AttractionMapping
{
    public static void Map(this EntityTypeBuilder<Attraction> builder)
    {
        builder.ToTable("Attractions");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).AsRequiredText(256);
        builder.Property(a => a.RiskSummer).AsDecimal(5, 2);
        builder.Property(a => a.RiskWinter).AsDecimal(5, 2);

        builder.HasOne(a => a.PostalCode)
              .WithMany(p => p.Attractions)
              .HasForeignKey(a => a.PostalCodeId)
              .HasPrincipalKey(p => p.Plz)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
