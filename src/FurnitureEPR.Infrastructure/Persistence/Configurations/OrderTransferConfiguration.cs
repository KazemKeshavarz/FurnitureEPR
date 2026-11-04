using FurnitureEPR.Model.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class OrderTransferConfiguration : IEntityTypeConfiguration<OrderTransfer>
{
    public void Configure(EntityTypeBuilder<OrderTransfer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.RequestedAtUtc).IsRequired();
        builder.HasOne(x => x.SourceOrder).WithMany().HasForeignKey(x => x.SourceOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetOrder).WithMany().HasForeignKey(x => x.TargetOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.SourceOrderId, x.RequestedAtUtc });
        builder.HasIndex(x => new { x.TargetOrderId, x.RequestedAtUtc });
    }
}

public sealed class OrderTransferLineConfiguration : IEntityTypeConfiguration<OrderTransferLine>
{
    public void Configure(EntityTypeBuilder<OrderTransferLine> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        builder.HasOne(x => x.Transfer).WithMany(x => x.Lines).HasForeignKey(x => x.OrderTransferId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.SourceOrderItem).WithMany().HasForeignKey(x => x.SourceOrderItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetOrderItem).WithMany().HasForeignKey(x => x.TargetOrderItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}
