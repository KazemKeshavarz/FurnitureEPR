using FurnitureEPR.Model.Components;
using MediatR;

namespace FurnitureEPR.Application.Features.Components.Commands.CreateComponent;

public sealed class CreateComponentCommandHandler : IRequestHandler<CreateComponentCommand, Guid>
{
    private readonly IComponentRepository _repository;
    public CreateComponentCommandHandler(IComponentRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CreateComponentCommand request, CancellationToken cancellationToken)
    {
        var component = new Component(request.Name);
        await _repository.AddAsync(component, cancellationToken);
        return component.Id;
    }
}
