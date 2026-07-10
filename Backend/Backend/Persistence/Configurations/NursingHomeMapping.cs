namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class NursingHomeMapping
{
    public static void Map(this EntityTypeBuilder<NursingHome> builder)
    {
        builder.ToTable("NursingHomes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Name).AsRequiredText(256);

        builder.HasOne(n => n.PostalCode)
              .WithMany(p => p.NursingHomes)
              .HasForeignKey(n => n.PostalCodeId)
              .HasPrincipalKey(p => p.Plz)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
