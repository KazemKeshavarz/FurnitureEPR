using FurnitureEPR.Model.Categories;
using MediatR;

namespace FurnitureEPR.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryCommandHandler(ICategoryRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name);
        await _repository.AddAsync(category, cancellationToken);
        return category.Id;
    }
}
