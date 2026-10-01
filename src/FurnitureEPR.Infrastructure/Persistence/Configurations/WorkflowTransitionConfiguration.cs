using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.HasOne(x => x.WorkflowVersion)
            .WithMany(x => x.Transitions)
            .HasForeignKey(x => x.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FromStage)
            .WithMany()
            .HasForeignKey(x => x.FromStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToStage)
            .WithMany()
            .HasForeignKey(x => x.ToStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.WorkflowVersionId, x.FromStageId, x.ToStageId }).IsUnique();
    }
}
