using FurnitureEPR.Model.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
