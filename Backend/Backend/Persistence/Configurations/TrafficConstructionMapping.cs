namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class TrafficConstructionMapping
{
    public static void Map(this EntityTypeBuilder<TrafficConstruction> builder)
    {
        builder.ToTable("TrafficConstructions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Severity).HasMaxLength(64);

        builder.HasOne(t => t.District)
              .WithMany(d => d.TrafficConstructions)
              .HasForeignKey(t => t.DistrictId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Segment)
              .WithMany(h => h.TrafficConstructions)
              .HasForeignKey(t => t.SegmentId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
