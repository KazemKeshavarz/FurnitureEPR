using FurnitureEPR.Model.Components;

namespace FurnitureEPR.Application.Features.Components;

public interface IComponentRepository
{
    Task AddAsync(Component component, CancellationToken cancellationToken);
}
