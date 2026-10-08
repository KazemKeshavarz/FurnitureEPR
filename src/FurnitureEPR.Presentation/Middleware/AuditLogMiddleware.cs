using System.Security.Claims;
using FurnitureEPR.Infrastructure.Persistence;
using FurnitureEPR.Model.Auditing;

namespace FurnitureEPR.Presentation.Middleware;

public sealed class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(
        RequestDelegate next,
        ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ApplicationDbContext db)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            try
            {
                Guid? userId = null;
                var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (Guid.TryParse(userIdValue, out var parsedUserId))
                    userId = parsedUserId;

                db.Set<AuditLog>().Add(
                    new AuditLog(
                        userId,
                        context.Request.Method,
                        context.Request.Path.Value ?? "/",
                        context.Response.StatusCode));

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // خطای ثبت Audit نباید پاسخ اصلی API را خراب کند.
                _logger.LogError(ex, "Failed to persist audit log.");
            }
        }
    }
}
