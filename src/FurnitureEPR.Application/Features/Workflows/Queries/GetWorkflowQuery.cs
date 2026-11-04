using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Queries;

public sealed record GetWorkflowQuery(Guid Id) : IRequest<WorkflowDetailDto?>;
