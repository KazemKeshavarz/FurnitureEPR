using MediatR;

namespace FurnitureEPR.Application.Features.Components.Queries;

public sealed record GetComponentQuery(Guid Id) : IRequest<ComponentDto?>;
