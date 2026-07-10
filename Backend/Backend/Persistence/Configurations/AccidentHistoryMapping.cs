namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class AccidentHistoryMapping
{
    public static void Map(this EntityTypeBuilder<AccidentHistory> builder)
    {
        builder.ToTable("AccidentHistories");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.DistrictBasis).HasMaxLength(256);

        builder.Property(a => a.RatePer10000Weighted).AsDecimal(8, 4);
        builder.Property(a => a.HotspotFactor).AsDecimal(8, 4);
        builder.Property(a => a.AccidentsHotspotAdjusted).AsDecimal(8, 2);

        builder.HasOne(a => a.District)
              .WithMany(d => d.AccidentHistories)
              .HasForeignKey(a => a.DistrictId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
