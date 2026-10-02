using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.PublishWorkflowVersion;

public sealed record PublishWorkflowVersionCommand(Guid WorkflowVersionId) : IRequest;
