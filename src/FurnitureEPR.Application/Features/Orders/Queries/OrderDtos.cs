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
    IReadOnlyCollection<OrderItemComponentDto> Components);

public sealed record OrderWorkflowHistoryDto(
    Guid Id,
    Guid FromStageId,
    Guid ToStageId,
    Guid TransitionId,
    DateTime OccurredAtUtc);

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
    IReadOnlyCollection<OrderWorkflowHistoryDto> History);

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? FinalizedAtUtc,
    Guid? CreatedByUserId,
    IReadOnlyCollection<OrderItemDto> Items,
    IReadOnlyCollection<OrderWorkflowInstanceDto> Workflows);