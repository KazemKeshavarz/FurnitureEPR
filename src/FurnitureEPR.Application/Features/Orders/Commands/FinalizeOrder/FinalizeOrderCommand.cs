using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.FinalizeOrder;

public sealed record FinalizeOrderCommand(Guid OrderId) : IRequest;
