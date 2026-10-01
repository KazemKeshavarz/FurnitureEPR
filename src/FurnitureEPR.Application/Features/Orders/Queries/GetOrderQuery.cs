using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed record GetOrderQuery(Guid Id) : IRequest<OrderDto?>;
