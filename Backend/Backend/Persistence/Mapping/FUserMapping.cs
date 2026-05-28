namespace Persistence.Mapping;

using Base.Persistence.Mappings;

using Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class FUserMapping
{
    public static void Map(this EntityTypeBuilder<FUser> entity)
    {
        entity.ToTable("FUser");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Username).AsRequiredText(128);
    }
}
