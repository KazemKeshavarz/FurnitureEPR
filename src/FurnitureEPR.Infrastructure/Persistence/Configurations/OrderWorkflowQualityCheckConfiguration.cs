using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class OrderWorkflowQualityCheckConfiguration
    : IEntityTypeConfiguration<OrderWorkflowQualityCheck>
{
    public void Configure(EntityTypeBuilder<OrderWorkflowQualityCheck> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Result)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.OrderWorkflowInstanceId, x.StageId, x.CheckedAtUtc });

        // QC بخشی از Runtime Instance است و با حذف Instance باید حذف شود.
        builder.HasOne(x => x.OrderWorkflowInstance)
            .WithMany(x => x.QualityChecks)
            .HasForeignKey(x => x.OrderWorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Stage فقط داده مرجع Workflow است و با حذف QC نباید حذف شود.
        builder.HasOne(x => x.Stage)
            .WithMany()
            .HasForeignKey(x => x.StageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
