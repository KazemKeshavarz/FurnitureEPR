using FurnitureEPR.Infrastructure.Identity;
using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class WorkflowStageConfiguration : IEntityTypeConfiguration<WorkflowStage>
{
    public void Configure(EntityTypeBuilder<WorkflowStage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.WorkflowVersionId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.WorkflowVersionId, x.SortOrder });
        builder.HasIndex(x => x.ResponsibleRoleId);

        builder.HasOne(x => x.WorkflowVersion)
            .WithMany(x => x.Stages)
            .HasForeignKey(x => x.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // اگر یک Role حذف شود، مرحله‌های گردشکار نباید بی‌صاحب شوند.
        builder.HasOne<ApplicationRole>()
            .WithMany()
            .HasForeignKey(x => x.ResponsibleRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
