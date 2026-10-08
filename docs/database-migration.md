# Database Migration

## ایجاد Migration اولیه

```powershell
dotnet ef migrations add InitialCreate --project src/FurnitureEPR.Infrastructure --startup-project src/FurnitureEPR.Presentation --output-dir Persistence/Migrations
```

## اعمال Migration روی SQL Server

```powershell
dotnet ef database update --project src/FurnitureEPR.Infrastructure --startup-project src/FurnitureEPR.Presentation
```

## پیش‌نیاز

اگر ابزار EF Core روی سیستم نصب نیست:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
```

یا در صورت نصب قبلی:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.0
```

## نکات امنیتی

- Connection String واقعی را در User Secrets، Environment Variables یا Secret Manager قرار دهید.
- مقدار `Jwt:Key` نباید در Repository واقعی استفاده شود.
- مقدار `IdentitySeed:AdminPassword` نیز فقط از Secret یا Environment Variable خوانده شود.
- Seed اولیه Admin در محیط Production فقط در صورت داشتن فرآیند کنترل‌شده فعال شود.

## وضعیت این Repository

Migration فایل‌محور هنوز در Repository تولید نشده است، چون تولید آن نیازمند اجرای واقعی EF CLI و دسترسی به SDK/Package Restore است. این مستندات فرمان دقیق و قابل اجرای محلی را نگه می‌دارند تا Migration بدون حدس و بدون تولید دستی SQL ساخته شود.