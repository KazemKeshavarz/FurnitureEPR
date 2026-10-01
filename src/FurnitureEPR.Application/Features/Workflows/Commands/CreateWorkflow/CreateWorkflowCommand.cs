using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflow;

public sealed record CreateWorkflowCommand(string Name, string Code) : IRequest<Guid>;
