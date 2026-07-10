namespace Persistence.Mapping;

using Base.Persistence.Mappings;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class IncidentTypeMapping
{
    public static void Map(this EntityTypeBuilder<IncidentType> builder)
    {
        builder.ToTable("IncidentTypes");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.IncidentTypeName).AsRequiredText(256);
        builder.HasIndex(i => i.IncidentTypeName).IsUnique();

        builder.Property(i => i.SeverityMean).AsDecimal(8, 4);
        builder.Property(i => i.SeverityMin).AsDecimal(8, 4);
        builder.Property(i => i.SeverityMax).AsDecimal(8, 4);
    }
}
