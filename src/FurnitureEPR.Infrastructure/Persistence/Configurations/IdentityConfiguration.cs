using FurnitureEPR.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FurnitureEPR.Infrastructure.Persistence.Configurations;

public sealed class IdentityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // جدول‌های Identity توسط IdentityDbContext ساخته می‌شوند؛
        // این Configuration فقط تنظیمات اختصاصی کاربر پروژه را نگه می‌دارد.
        builder.ToTable("Users");
    }
}
