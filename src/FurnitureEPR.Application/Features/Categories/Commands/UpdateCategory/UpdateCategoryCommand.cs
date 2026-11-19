using MediatR;
namespace FurnitureEPR.Application.Features.Categories.Commands.UpdateCategory;
public sealed record UpdateCategoryCommand(Guid Id, string Name) : IRequest;
