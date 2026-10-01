using FurnitureEPR.Model.Products;
using MediatR;

namespace FurnitureEPR.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.CategoryId, request.Name);
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
