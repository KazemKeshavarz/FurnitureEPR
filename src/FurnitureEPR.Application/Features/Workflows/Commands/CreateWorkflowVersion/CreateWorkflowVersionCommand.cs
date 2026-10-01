using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflowVersion;

public sealed record CreateWorkflowVersionCommand(Guid WorkflowId, int VersionNumber) : IRequest<Guid>;
