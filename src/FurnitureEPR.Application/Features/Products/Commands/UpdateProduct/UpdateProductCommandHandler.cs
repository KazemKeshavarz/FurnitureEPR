using MediatR;
namespace FurnitureEPR.Application.Features.Products.Commands.UpdateProduct;
public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _repository;
    public UpdateProductCommandHandler(IProductRepository repository) => _repository = repository;
    public Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        => _repository.UpdateAsync(request.Id, request.CategoryId, request.Name, cancellationToken);
}
