using FurnitureEPR.Model.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class OrderItemWarehouseAllocationConfiguration : IEntityTypeConfiguration<OrderItemWarehouseAllocation>
{
    public void Configure(EntityTypeBuilder<OrderItemWarehouseAllocation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.AssignedAtUtc).IsRequired();
        builder.HasOne(x => x.OrderItem).WithMany(x => x.WarehouseAllocations)
            .HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Warehouse).WithMany()
            .HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OrderItemId, x.WarehouseId }).IsUnique();
    }
}
