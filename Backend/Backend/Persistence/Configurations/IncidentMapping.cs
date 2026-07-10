namespace Persistence.Mapping;

using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class IncidentMapping
{
    public static void Map(this EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.PostalCode)
              .WithMany(p => p.Incidents)
              .HasForeignKey(i => i.PostalCodeId)
              .HasPrincipalKey(p => p.Plz)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.LocationType)
              .WithMany(l => l.Incidents)
              .HasForeignKey(i => i.LocationTypeId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.IncidentType)
              .WithMany(it => it.Incidents)
              .HasForeignKey(i => i.IncidentTypeId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Attraction)
            .WithMany(a => a.Incidents)
            .HasForeignKey(i => i.AttractionId)
             .OnDelete(DeleteBehavior.Restrict);
    }
}
