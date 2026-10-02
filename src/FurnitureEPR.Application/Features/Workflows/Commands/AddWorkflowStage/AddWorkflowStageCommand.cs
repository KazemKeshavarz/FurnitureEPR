using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowStage;

public sealed record AddWorkflowStageCommand(
    Guid WorkflowVersionId,
    string Name,
    string Code,
    int SortOrder,
    bool RequiresQualityControl,
    Guid? ResponsibleRoleId) : IRequest<Guid>;
