# Database Migration

## اعمال Migrationهای موجود

Migrationهای اولیه و `AddWarehouseTransferAndProductionBatches` در مخزن ثبت شده‌اند. برای ساخت دیتابیس جدید یا به‌روزرسانی دیتابیس موجود، از دستور زیر استفاده کنید:

```powershell
dotnet ef database update --project src/FurnitureEPR.Infrastructure --startup-project src/FurnitureEPR.Presentation
```

برای مشاهده فهرست Migrationها:

```powershell
dotnet ef migrations list --project src/FurnitureEPR.Infrastructure --startup-project src/FurnitureEPR.Presentation
```

## اعمال Migration روی SQL Server

```powershell
dotnet ef database update --project src/FurnitureEPR.Infrastructure --startup-project src/FurnitureEPR.Presentation
```

## پیش‌نیاز

اگر ابزار EF Core روی سیستم نصب نیست:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
```

یا در صورت نصب قبلی:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.13
```

## نکات امنیتی

- Connection String واقعی را در User Secrets، Environment Variables یا Secret Manager قرار دهید.
- مقدار `Jwt:Key` نباید در Repository واقعی استفاده شود.
- مقدار `IdentitySeed:AdminPassword` نیز فقط از Secret یا Environment Variable خوانده شود.
- Seed اولیه Admin در محیط Production فقط در صورت داشتن فرآیند کنترل‌شده فعال شود.

## تغییرات جدید مدل داده

مدل فعلی علاوه بر سفارش و انبار، شامل جداول `OrderTransfers` و `OrderTransferLines` برای نگهداری درخواست‌های انتقال و سوابق اقلام آن است. همچنین `OrderItemProductionBatches` تعداد هر قلم را به بخش‌های مستقل تولیدی/انتقالی تقسیم می‌کند تا مقدار آماده، در حال تولید و باقی‌مانده جداگانه قابل محاسبه باشد. وضعیت `HasShortage` نیز به وضعیت‌های سفارش اضافه شده است. Migration `AddWarehouseTransferAndProductionBatches` جداول انبار، تخصیص، درخواست انتقال، ردیف‌های انتقال و بخش‌های تولید را ایجاد می‌کند. همچنین امکان اجرای چند گردش‌کار برای یک دسته‌بندی سفارش را فراهم می‌کند تا مقدار جبرانی، مستقل از تولید اولیه پیگیری شود. این Migration را روی دیتابیس موجود با `database update` اعمال کنید؛ Migration اولیه را بازنویسی نکنید.

## بررسی پس از اعمال

پس از اجرای Migration، وجود جداول `Warehouses`، `OrderItemWarehouseAllocations`، `OrderTransfers`، `OrderTransferLines` و `OrderItemProductionBatches` و ستون nullable به نام `ProductionBatchId` در `OrderWorkflowInstances` را بررسی کنید. اجرای موفق Build/Tests در CI به معنی اعمال Migration روی SQL Server نیست؛ ابتدا روی دیتابیس آزمایشی اعمال و سپس روی محیط اصلی اجرا شود.