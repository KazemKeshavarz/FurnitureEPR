# Workflow Runtime Architecture

## هدف

این سند توضیح می‌دهد گردشکار از یک **تعریف قابل تنظیم توسط Admin** چگونه به یک **گردشکار واقعی روی سفارش** تبدیل می‌شود.

اصل مهم این طراحی این است که Workflow Definition با Workflow Runtime یکی نیست:

- Definition مشخص می‌کند چه مرحله‌ها و چه مسیرهایی وجود دارند.
- Runtime مشخص می‌کند یک سفارش مشخص الان در کدام مرحله قرار دارد و چه مسیری را طی کرده است.

## معماری کلی

```text
Category
   │
   └── Published WorkflowVersion
             │
             ├── WorkflowStage
             └── WorkflowTransition

Order
   │
   ├── OrderItem
   │      └── Product
   │             └── Category
   │
   └── OrderWorkflowInstance
          │
          ├── WorkflowVersionId   ← Snapshot نسخه
          ├── CurrentStageId
          ├── Status
          └── History
                 │
                 └── WorkflowTransition
```

یک سفارش می‌تواند چند Product داشته باشد. چون Product فقط به یک Category تعلق دارد و Workflow به Category اختصاص داده می‌شود، یک Order در صورت داشتن Product از چند Category می‌تواند برای هر Category یک `OrderWorkflowInstance` مستقل داشته باشد.

## چرا Snapshot کردن WorkflowVersion مهم است؟

Workflow یک Category ممکن است در آینده تغییر کند.

مثلاً:

```text
نسخه 1
ثبت سفارش → تولید → کنترل کیفیت → انبار

نسخه 2
ثبت سفارش → آماده‌سازی → تولید → کنترل کیفیت → انبار
```

اگر سفارش قدیمی با نسخه 1 ثبت شده باشد، نباید با تغییر Category ناگهان وارد نسخه 2 شود.

بنابراین هنگام Finalize سفارش:

1. Category مربوط به Productهای سفارش پیدا می‌شود.
2. WorkflowVersion منتشرشده همان لحظه پیدا می‌شود.
3. شناسه آن نسخه داخل `OrderWorkflowInstance` ذخیره می‌شود.
4. از آن لحظه Runtime سفارش به همان نسخه وابسته است.

## Lifecycle

### 1. Draft

در این مرحله سفارش هنوز وارد گردشکار عملیاتی نشده است.

`Order.Status = Draft`

### 2. Finalize

وقتی سفارش Finalize می‌شود:

```text
Draft
  ↓
Validate Order
  ↓
Find Categories
  ↓
Find Published WorkflowVersion
  ↓
Find Initial Active Stage
  ↓
Create OrderWorkflowInstance
  ↓
Order.Status = Active
```

در پیاده‌سازی فعلی، اولین Stage فعال بر اساس کمترین `SortOrder` انتخاب می‌شود.

این تصمیم فعلاً ساده و قابل فهم است؛ در آینده اگر نیاز به چند نقطه ورود یا Branchهای موازی داشته باشیم، مدل Entry Point توسعه داده می‌شود.

## OrderWorkflowInstance

`OrderWorkflowInstance` وضعیت فعلی اجرای Workflow برای یک Order و Category را نگه می‌دارد.

اطلاعات اصلی:

- `OrderId`
- `CategoryId`
- `WorkflowVersionId`
- `CurrentStageId`
- `Status`
- `StartedAtUtc`
- `CompletedAtUtc`

وضعیت‌های فعلی:

```text
Active
Completed
Cancelled
```

## اجرای Transition

وقتی کاربر یا سیستم می‌خواهد سفارش را از مرحله فعلی به مرحله بعد منتقل کند:

```text
Order
  ↓
OrderWorkflowInstance
  ↓
CurrentStage
  ↓
Requested WorkflowTransition
  ↓
Validate
  ├── Transition belongs to same WorkflowVersion
  ├── FromStage == CurrentStage
  └── ToStage is active
  ↓
MoveTo
  ↓
Create History
```

Endpoint فعلی:

`POST /api/orders/{orderId}/workflow/{categoryId}/transitions/{transitionId}`

این endpoint عمداً Transition را دریافت می‌کند، نه نام مرحله مقصد را؛ بنابراین حرکت سفارش فقط از مسیرهایی انجام می‌شود که Admin در Workflow Definition تعریف کرده است.

## Completion

هر `OrderWorkflowInstance` می‌تواند به‌صورت مستقل کامل شود.

```text
Workflow Instance
      ↓
Complete
      ↓
آیا Workflow فعال دیگری برای Order وجود دارد؟
   ├── بله → Order همچنان Active
   └── خیر → Order = Completed
```

Endpoint فعلی:

`POST /api/orders/{orderId}/workflow/{categoryId}/complete`

بنابراین در سفارش‌های دارای چند Category، پایان یک Workflow باعث تکمیل زودهنگام کل Order نمی‌شود.

---

## History

`OrderWorkflowHistory` برای Audit و پیگیری مسیر سفارش استفاده می‌شود.

هر رکورد شامل:

- `OrderWorkflowInstanceId`
- `FromStageId`
- `ToStageId`
- `TransitionId`
- `OccurredAtUtc`

مثال:

```text
Stage A
  ↓ Transition 1
Stage B
  ↓ Transition 4
Stage C
```

History باعث می‌شود بتوانیم بفهمیم سفارش از چه مسیر واقعی‌ای عبور کرده است.

## Identity و Claims

احراز هویت Runtime بر پایه ASP.NET Core Identity و JWT انجام می‌شود.

Claims اصلی Token:

- `ClaimTypes.NameIdentifier` → شناسه User
- `ClaimTypes.Name` → نام کاربر
- `ClaimTypes.Role` → نام Role
- `role_id` → شناسه Guid همان Role

وجود `role_id` مهم است چون WorkflowStage به‌جای وابستگی به Identity، فقط `ResponsibleRoleId` را نگه می‌دارد.

Runtime هنگام Transition، QC و Complete بررسی می‌کند که Role شناسه‌شده در Claimهای کاربر با `ResponsibleRoleId` مرحله فعلی منطبق باشد.

Endpointهای Identity:

```text
POST /api/auth/login
GET  /api/auth/me
```

`/api/auth/me` برای بررسی Claims فعلی در زمان توسعه در نظر گرفته شده است.

یک Seeder اختیاری نیز برای ساخت Role و Administrator اولیه وجود دارد و با `IdentitySeed:Enabled` فعال می‌شود. Password نباید در Repository نگهداری شود و در محیط واقعی باید از User Secrets یا Environment Variables تأمین شود.

---

## QC

اگر یک Stage با `RequiresQualityControl = true` تعریف شده باشد، Runtime اجازه عبور از آن Stage را بدون Approval نمی‌دهد.

فرآیند:

```text
Current Stage
     ↓
Requires QC?
  ├── No  → Transition
  └── Yes
       ↓
      QC
    ├── Approve → Transition مجاز
    └── Reject  → همان Stage
                    ↓
                  اصلاح
                    ↓
                    QC مجدد
```

هر بار QC یک `OrderWorkflowQualityCheck` مستقل ثبت می‌کند:

- Stage
- Result
- Comment
- CheckedAtUtc
- CheckedByUserId

بنابراین اگر یک Stage چند بار Reject و بعد Approve شود، تمام Attemptها قابل مشاهده هستند.

Endpoint فعلی:

`POST /api/orders/{orderId}/workflow/{categoryId}/quality-control`

بدنه شامل نتیجه QC است:

```json
{
  "result": "Approved",
  "comment": "..."
}
```

در حالت Reject، CurrentStage تغییر نمی‌کند. Approval فقط آخرین QC همان Stage را معتبر می‌کند و سپس Transition می‌تواند اجرا شود.

**محدودیت فعلی:** `CheckedByUserId` هنوز از هویت کاربر احراز هویت‌شده پر نمی‌شود و باید در مرحله سخت‌سازی Authorization متصل شود. تا زمان تکمیل و آزمون این اتصال، ثبت هویت ممیز برای بهره‌برداری عملیاتی تأییدشده نیست.

---

## Parallel Production

بعضی فرآیندهای واقعی ممکن است چند Stage را هم‌زمان اجرا کنند.

```text
             ┌── Stage A ──┐
Start ───────┤             ├── Next
             └── Stage B ──┘
```

مدل فعلی `OrderWorkflowInstance.CurrentStageId` برای Parallel کامل کافی نیست.

بنابراین Parallel Execution عمداً در این مرحله پیاده نشده است. وقتی نیاز واقعی آن مشخص شود، Runtime به مدل Branch/WorkItem توسعه داده خواهد شد، بدون اینکه Workflow Definition فعلی مجبور به Hard-code کردن Stageها شود.

## قوانین مهم فعلی

1. Product فقط به یک Category تعلق دارد.
2. Workflow به Category اختصاص داده می‌شود.
3. فقط WorkflowVersion منتشرشده می‌تواند به Category اختصاص داده شود.
4. WorkflowVersion منتشرشده قابل تغییر نیست.
5. سفارش هنگام Finalize نسخه Workflow را Snapshot می‌کند.
6. تغییر Workflow Category روی سفارش‌های قبلی اثر نمی‌گذارد.
7. هر Order می‌تواند برای هر Category یک Runtime Instance داشته باشد.
8. Transition فقط در صورتی اجرا می‌شود که از CurrentStage شروع شود.
9. مسیر Transition باید متعلق به همان WorkflowVersion باشد.
10. History حرکت‌های Runtime را ثبت می‌کند.
11. QC و Parallel هنوز لایه بعدی توسعه هستند.

## وضعیت پیاده‌سازی

این سند وضعیت کد را توصیف می‌کند، نه تأیید آمادگی بهره‌برداری. CI ساخت بک‌اند و فرانت‌اند در آخرین اجرا موفق بوده است؛ با این حال، آزمون end-to-end روی پایگاه داده و محیط واقعی هنوز باید انجام شود.

### انجام شده

- `OrderWorkflowInstance`
- `OrderWorkflowInstanceStatus`
- `OrderWorkflowHistory`
- EF Core configurations
- ثبت Runtime در `ApplicationDbContext`
- شروع Runtime هنگام Finalize
- Snapshot کردن WorkflowVersion
- اجرای Transition
- ثبت History
- API اجرای Transition
- Read API سفارش همراه با Current Stage و History
- API تکمیل Workflow و تکمیل خودکار Order پس از پایان همه Workflowها
- QC قابل تکرار با ثبت تمام Attemptها
- ثبت Repository در DI

### باقی‌مانده / نیازمند تأیید

1. تست‌های Domain و Integration برای Transition، QC، مجوزها و Completion.
2. اتصال قطعی `CheckedByUserId` به کاربر احراز هویت‌شده و بازبینی مجوزهای هر endpoint حساس.
3. اجرای migration و آزمون روی SQL Server محیط مقصد.
4. آزمون end-to-end از ثبت سفارش تا تکمیل تولید و تحویل.
5. مدل Parallel Branch / WorkItem فقط در صورت تأیید نیاز واقعی کارگاه؛ در حال حاضر اجرای موازی پیاده نشده است.

## اصل طراحی

تا زمانی که Workflow واقعی کارگاه به‌صورت دقیق مشخص نشده است، Stageهای واقعی در کد Hard-code نمی‌شوند.

نام Stage، ترتیب و Transitionها باید از طریق Workflow Definition قابل تنظیم باشند و Runtime فقط همان Definition منتشرشده را اجرا کند.