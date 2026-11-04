using FurnitureEPR.Model.Workflow;
using FurnitureEPR.Application.Features.Workflows.Queries;

namespace FurnitureEPR.Application.Features.Workflows;

public interface IWorkflowRepository
{
    Task<WorkflowPagedResult<WorkflowListItemDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
    Task<WorkflowDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
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

    Task UpdateStageAsync(Guid workflowVersionId, Guid stageId, string name, string code, int sortOrder, bool requiresQualityControl, Guid? responsibleRoleId, CancellationToken cancellationToken);
    Task DeleteStageAsync(Guid workflowVersionId, Guid stageId, CancellationToken cancellationToken);
    Task UpdateTransitionAsync(Guid workflowVersionId, Guid transitionId, Guid fromStageId, Guid toStageId, string name, CancellationToken cancellationToken);
    Task DeleteTransitionAsync(Guid workflowVersionId, Guid transitionId, CancellationToken cancellationToken);
    // بعد از انتشار، ساختار نسخه دیگر قابل تغییر نیست.
    Task PublishVersionAsync(
        Guid workflowVersionId,
        CancellationToken cancellationToken);
}
