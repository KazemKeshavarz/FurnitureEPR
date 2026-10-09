namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed record OrderItemComponentDto(
    Guid Id,
    Guid ComponentId,
    string ComponentName,
    decimal Quantity);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    IReadOnlyCollection<OrderItemComponentDto> Components);

// نام مراحل و Transition را هم نگه می‌داریم تا تاریخچه برای کاربر نهایی قابل فهم باشد.
public sealed record OrderWorkflowHistoryDto(
    Guid Id,
    Guid FromStageId,
    string FromStageName,
    Guid ToStageId,
    string ToStageName,
    Guid TransitionId,
    string TransitionName,
    DateTime OccurredAtUtc);

public sealed record OrderWorkflowQualityCheckDto(
    Guid Id,
    Guid StageId,
    string StageName,
    string Result,
    string? Comment,
    DateTime CheckedAtUtc,
    Guid? CheckedByUserId);

public sealed record OrderWorkflowInstanceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid WorkflowVersionId,
    int WorkflowVersionNumber,
    Guid CurrentStageId,
    string CurrentStageName,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    IReadOnlyCollection<OrderWorkflowHistoryDto> History,
    IReadOnlyCollection<OrderWorkflowQualityCheckDto> QualityChecks);

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? FinalizedAtUtc,
    Guid? CreatedByUserId,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    IReadOnlyCollection<OrderItemDto> Items,
    IReadOnlyCollection<OrderWorkflowInstanceDto> Workflows);