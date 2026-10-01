using FurnitureEPR.Model.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class ProductComponentConfiguration : IEntityTypeConfiguration<ProductComponent>
{
    public void Configure(EntityTypeBuilder<ProductComponent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DefaultQuantity).HasPrecision(18, 3).IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Components)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Component)
            .WithMany(x => x.ProductComponents)
            .HasForeignKey(x => x.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ProductId, x.ComponentId }).IsUnique();
    }
}
