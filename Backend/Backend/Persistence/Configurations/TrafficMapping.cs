namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class TrafficMapping
{
    public static void Map(this EntityTypeBuilder<Traffic> builder)
    {
        builder.ToTable("Traffics");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.AverageVehicleSpeed).AsDecimal(6, 2);
        builder.Property(t => t.TravelTime).AsDecimal(8, 2);
        builder.Property(t => t.TrafficStatus).HasMaxLength(64);

        builder.HasOne(t => t.District)
              .WithMany(d => d.Traffics)
              .HasForeignKey(t => t.DistrictId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Segment)
              .WithMany(h => h.Traffics)
              .HasForeignKey(t => t.SegmentId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
