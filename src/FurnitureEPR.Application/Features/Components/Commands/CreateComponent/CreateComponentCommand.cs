using MediatR;

namespace FurnitureEPR.Application.Features.Components.Commands.CreateComponent;

public sealed record CreateComponentCommand(string Name) : IRequest<Guid>;
