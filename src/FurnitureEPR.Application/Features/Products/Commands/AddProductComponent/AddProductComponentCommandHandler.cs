using FurnitureEPR.Application.Features.Products;
using MediatR;

namespace FurnitureEPR.Application.Features.Products.Commands.AddProductComponent;

public sealed class AddProductComponentCommandHandler : IRequestHandler<AddProductComponentCommand>
{
    private readonly IProductComponentRepository _repository;

    public AddProductComponentCommandHandler(IProductComponentRepository repository)
        => _repository = repository;

    public Task Handle(AddProductComponentCommand request, CancellationToken cancellationToken)
        => _repository.AddAsync(
            request.ProductId,
            request.ComponentId,
            request.DefaultQuantity,
            cancellationToken);
}
