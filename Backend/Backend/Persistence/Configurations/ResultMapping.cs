namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class ResultMapping
{
    public static void Map(this EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("Results");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RiskScore).AsDecimal(8, 4);
        builder.Property(r => r.RiskLevel).HasMaxLength(64);
        builder.Property(r => r.ScoreTraffic).AsDecimal(8, 4);
        builder.Property(r => r.ScoreAccident).AsDecimal(8, 4);
        builder.Property(r => r.ScoreWeather).AsDecimal(8, 4);
        builder.Property(r => r.ScoreHoliday).AsDecimal(8, 4);
        builder.Property(r => r.ScoreEvent).AsDecimal(8, 4);
        builder.Property(r => r.Explanation).HasMaxLength(2048);
        builder.Property(r => r.ModelVersion).HasMaxLength(64);
        builder.Property(r => r.Confidence).AsDecimal(8, 4);

        builder.HasOne(r => r.District)
              .WithMany(d => d.Results)
              .HasForeignKey(r => r.DistrictId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
