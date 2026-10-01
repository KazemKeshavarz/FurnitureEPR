using FurnitureEPR.Model.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace FurnitureEPR.Infrastructure.Persistence.Configurations;
public sealed class OrderItemComponentConfiguration : IEntityTypeConfiguration<OrderItemComponent>
{
    public void Configure(EntityTypeBuilder<OrderItemComponent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ComponentName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        builder.HasOne(x => x.OrderItem).WithMany(x => x.Components).HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OrderItem>().WithMany().HasForeignKey(x => x.OrderItemId);
    }
}
