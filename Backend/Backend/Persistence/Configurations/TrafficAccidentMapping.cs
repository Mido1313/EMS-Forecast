namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class TrafficAccidentMapping
{
    public static void Map(this EntityTypeBuilder<TrafficAccident> builder)
    {
        builder.ToTable("TrafficAccidents");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Severity).HasMaxLength(64);

        builder.HasOne(t => t.District)
              .WithMany(d => d.TrafficAccidents)
              .HasForeignKey(t => t.DistrictId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Segment)
              .WithMany(h => h.TrafficAccidents)
              .HasForeignKey(t => t.SegmentId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
