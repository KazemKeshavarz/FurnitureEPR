using MediatR;

namespace FurnitureEPR.Application.Features.Categories.Queries;

public sealed record GetCategoryQuery(Guid Id) : IRequest<CategoryDto?>;
