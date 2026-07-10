namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class MunicipalityMapping
{
    public static void Map(this EntityTypeBuilder<Municipality> builder)
    {
        builder.ToTable("Municipalities");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MunicipalityName).AsRequiredText(256);

        builder.HasOne(m => m.PostalCode)
              .WithMany(p => p.Municipalities)
              .HasForeignKey(m => m.PostalCodeId)
              .HasPrincipalKey(p => p.Plz)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
