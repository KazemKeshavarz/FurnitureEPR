using FurnitureEPR.Model.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class OrderItemProductionBatchConfiguration : IEntityTypeConfiguration<OrderItemProductionBatch>
{
    public void Configure(EntityTypeBuilder<OrderItemProductionBatch> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.Origin).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasOne(x => x.OrderItem).WithMany(x => x.ProductionBatches)
            .HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.OrderItemId, x.Status });
        builder.HasIndex(x => x.SourceTransferLineId).IsUnique().HasFilter("[SourceTransferLineId] IS NOT NULL");
        builder.HasIndex(x => x.ReplenishmentWarehouseId);
        builder.HasOne<FurnitureEPR.Model.Warehouses.Warehouse>().WithMany().HasForeignKey(x => x.ReplenishmentWarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}
