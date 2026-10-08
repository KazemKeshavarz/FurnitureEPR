using FurnitureEPR.Model.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Method)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Path)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => new { x.OccurredAtUtc, x.UserId });
        builder.HasIndex(x => x.StatusCode);
    }
}
