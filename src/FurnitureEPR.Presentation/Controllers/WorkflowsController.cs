using FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowStage;
using FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowTransition;
using FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflow;
using FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflowVersion;
using FurnitureEPR.Application.Features.Workflows.Commands.PublishWorkflowVersion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FurnitureEPR.Application.Security;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/workflows")]
[Authorize(Policy = PermissionNames.WorkflowMove)]
public sealed class WorkflowsController : ControllerBase
{
    private readonly ISender _sender;

    public WorkflowsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        CreateWorkflowCommand command,
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(command, cancellationToken));

    [HttpPost("{workflowId:guid}/versions")]
    public async Task<ActionResult<Guid>> CreateVersion(
        Guid workflowId,
        CreateWorkflowVersionCommand command,
        CancellationToken cancellationToken)
    {
        if (workflowId != command.WorkflowId)
            return BadRequest("Route id and WorkflowId must match.");

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpPost("versions/{versionId:guid}/stages")]
    public async Task<ActionResult<Guid>> AddStage(
        Guid versionId,
        AddWorkflowStageCommand command,
        CancellationToken cancellationToken)
    {
        if (versionId != command.WorkflowVersionId)
            return BadRequest("Route id and WorkflowVersionId must match.");

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpPost("versions/{versionId:guid}/transitions")]
    public async Task<ActionResult<Guid>> AddTransition(
        Guid versionId,
        AddWorkflowTransitionCommand command,
        CancellationToken cancellationToken)
    {
        if (versionId != command.WorkflowVersionId)
            return BadRequest("Route id and WorkflowVersionId must match.");

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpPost("versions/{versionId:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid versionId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PublishWorkflowVersionCommand(versionId),
            cancellationToken);

        return NoContent();
    }
}
