using MediatR;

namespace FurnitureEPR.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name) : IRequest<Guid>;
