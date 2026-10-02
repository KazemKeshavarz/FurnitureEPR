using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class OrderWorkflowHistoryConfiguration : IEntityTypeConfiguration<OrderWorkflowHistory>
{
    public void Configure(EntityTypeBuilder<OrderWorkflowHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.OrderWorkflowInstanceId, x.OccurredAtUtc });

        // تاریخچه بخشی از Runtime Instance است و با حذف آن باید حذف شود.
        builder.HasOne(x => x.OrderWorkflowInstance)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.OrderWorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Stage و Transition داده‌های تعریف گردشکار هستند و نباید با حذف تاریخچه حذف شوند.
        builder.HasOne(x => x.FromStage)
            .WithMany()
            .HasForeignKey(x => x.FromStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToStage)
            .WithMany()
            .HasForeignKey(x => x.ToStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Transition)
            .WithMany()
            .HasForeignKey(x => x.TransitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
