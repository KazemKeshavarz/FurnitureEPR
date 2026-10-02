using FurnitureEPR.Model.Workflow;

namespace FurnitureEPR.Application.Features.Workflows;

public interface IWorkflowRepository
{
    Task AddAsync(WorkflowDefinition workflow, CancellationToken cancellationToken);
    Task AddVersionAsync(WorkflowVersion version, CancellationToken cancellationToken);

    // نقش مسئول مرحله از Identity می‌آید، ولی خود Domain فقط شناسه نقش را نگه می‌دارد.
    Task<Guid> AddStageAsync(
        Guid workflowVersionId,
        string name,
        string code,
        int sortOrder,
        bool requiresQualityControl,
        Guid? responsibleRoleId,
        CancellationToken cancellationToken);

    // مسیر حرکت سفارش بین دو مرحله را در نسخه مشخص گردشکار ثبت می‌کند.
    Task<Guid> AddTransitionAsync(
        Guid workflowVersionId,
        Guid fromStageId,
        Guid toStageId,
        string name,
        CancellationToken cancellationToken);

    // بعد از انتشار، ساختار نسخه دیگر قابل تغییر نیست.
    Task PublishVersionAsync(
        Guid workflowVersionId,
        CancellationToken cancellationToken);
}
