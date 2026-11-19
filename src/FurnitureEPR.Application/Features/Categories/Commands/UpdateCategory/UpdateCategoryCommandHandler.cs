using MediatR;
namespace FurnitureEPR.Application.Features.Categories.Commands.UpdateCategory;
public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _repository;
    public UpdateCategoryCommandHandler(ICategoryRepository repository) => _repository = repository;
    public Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        => _repository.UpdateAsync(request.Id, request.Name, cancellationToken);
}
