namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed record ProductionTaskTransitionDto(
    Guid Id,
    string Name,
    Guid ToStageId,
    string ToStageName);

public sealed record ProductionTaskDto(
    Guid WorkflowInstanceId,
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    Guid CategoryId,
    string CategoryName,
    Guid CurrentStageId,
    string CurrentStageName,
    string CurrentStageCode,
    bool RequiresQualityControl,
    bool QualityControlApproved,
    bool CanMove,
    bool CanComplete,
    IReadOnlyCollection<ProductionTaskTransitionDto> Transitions,
    DateTime StartedAtUtc);

public sealed record PagedProductionTaskDto(
    IReadOnlyCollection<ProductionTaskDto> Items,
    int Page,
    int PageSize,
    int TotalCount);