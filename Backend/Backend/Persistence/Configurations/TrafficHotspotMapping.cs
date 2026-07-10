namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class TrafficHotspotMapping
{
    public static void Map(this EntityTypeBuilder<TrafficHotspot> builder)
    {
        builder.ToTable("TrafficHotspots");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.LinkId).AsRequiredText(128);
        builder.Property(t => t.HotspotName).AsRequiredText(256);
        builder.Property(t => t.RoadType).HasMaxLength(64);

        builder.HasIndex(t => t.LinkId).IsUnique();

        builder.Property(t => t.CriticalityWeight).AsDecimal(8, 4);
        builder.Property(t => t.FreeFlowSpeedKph).AsDecimal(6, 2);
        builder.Property(t => t.LengthKm).AsDecimal(8, 3);

        builder.HasOne(t => t.District)
              .WithMany(d => d.TrafficHotspots)
              .HasForeignKey(t => t.DistrictId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
