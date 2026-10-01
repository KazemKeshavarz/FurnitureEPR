using FurnitureEPR.Model.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasOne(x => x.WorkflowVersion)
            .WithMany()
            .HasForeignKey(x => x.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
