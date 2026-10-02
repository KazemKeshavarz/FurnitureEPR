using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Application.Security;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.RecordQualityControl;

public sealed class RecordQualityControlCommandHandler
    : IRequestHandler<RecordQualityControlCommand>
{
    private readonly IOrderWorkflowRuntimeRepository _repository;
    private readonly ICurrentUser _currentUser;

    public RecordQualityControlCommandHandler(
        IOrderWorkflowRuntimeRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public Task Handle(
        RecordQualityControlCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAccessException("Authenticated user is required.");

        return _repository.RecordQualityControlAsync(
            request.OrderId,
            request.CategoryId,
            request.Result,
            request.Comment,
            _currentUser.UserId,
            cancellationToken);
    }
}
