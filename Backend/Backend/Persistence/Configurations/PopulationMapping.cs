namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class PopulationMapping
{
    public static void Map(this EntityTypeBuilder<Population> builder)
    {
        builder.ToTable("Populations");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AgeStructure).HasMaxLength(512);

        builder.HasOne(p => p.Municipality)
              .WithMany(m => m.Populations)
              .HasForeignKey(p => p.MunicipalityId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
