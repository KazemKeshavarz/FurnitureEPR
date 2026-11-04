namespace FurnitureEPR.Application.Features.Workflows.Queries;

public sealed record WorkflowListItemDto(
    Guid Id,
    string Name,
    string Code,
    int VersionCount,
    int PublishedVersionCount,
    IReadOnlyCollection<string> AssignedCategoryNames);

public sealed record WorkflowStageDto(
    Guid Id,
    string Name,
    string Code,
    int SortOrder,
    bool RequiresQualityControl,
    Guid? ResponsibleRoleId,
    bool IsActive);

public sealed record WorkflowTransitionDto(
    Guid Id,
    Guid FromStageId,
    Guid ToStageId,
    string Name);

public sealed record WorkflowVersionDto(
    Guid Id,
    int VersionNumber,
    bool IsPublished,
    IReadOnlyCollection<WorkflowStageDto> Stages,
    IReadOnlyCollection<WorkflowTransitionDto> Transitions);

public sealed record WorkflowCategoryAssignmentDto(
    Guid CategoryId,
    string CategoryName,
    Guid WorkflowVersionId,
    int WorkflowVersionNumber);

public sealed record WorkflowDetailDto(
    Guid Id,
    string Name,
    string Code,
    IReadOnlyCollection<WorkflowCategoryAssignmentDto> AssignedCategories,
    IReadOnlyCollection<WorkflowVersionDto> Versions);
