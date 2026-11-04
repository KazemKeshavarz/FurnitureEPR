using FurnitureEPR.Model.Workflow;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.RecordQualityControl;

public sealed record RecordQualityControlCommand(
    Guid WorkflowInstanceId,
    QualityControlResult Result,
    string? Comment) : IRequest;
