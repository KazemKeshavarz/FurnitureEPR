using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.VersionNumber }).IsUnique();

        builder.HasOne(x => x.WorkflowDefinition)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
