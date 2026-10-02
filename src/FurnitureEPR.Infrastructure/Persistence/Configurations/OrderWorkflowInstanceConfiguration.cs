using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class OrderWorkflowInstanceConfiguration : IEntityTypeConfiguration<OrderWorkflowInstance>
{
    public void Configure(EntityTypeBuilder<OrderWorkflowInstance> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => new { x.OrderId, x.CategoryId }).IsUnique();
        builder.HasIndex(x => x.WorkflowVersionId);
        builder.HasIndex(x => x.CurrentStageId);
        builder.HasIndex(x => new { x.Status, x.CurrentStageId });

        // هر Instance به یک سفارش تعلق دارد و حذف سفارش باید Runtime آن را هم حذف کند.
        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category و نسخه گردشکار، داده‌های مرجع هستند و نباید با حذف Instance حذف شوند.
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WorkflowVersion)
            .WithMany()
            .HasForeignKey(x => x.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CurrentStage)
            .WithMany()
            .HasForeignKey(x => x.CurrentStageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
