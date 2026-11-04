using FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowStage;
using FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowTransition;
using FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflow;
using FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflowVersion;
using FurnitureEPR.Application.Features.Workflows.Commands.PublishWorkflowVersion;
using FurnitureEPR.Application.Features.Workflows.Commands;
using FurnitureEPR.Application.Features.Workflows.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FurnitureEPR.Application.Security;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/workflows")]
[Authorize(Policy = PermissionNames.WorkflowManage)]
public sealed class WorkflowsController : ControllerBase
{
    private readonly ISender _sender;

    public WorkflowsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<WorkflowPagedResult<WorkflowListItemDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
        => Ok(await _sender.Send(
            new GetWorkflowsQuery(page, pageSize, search),
            cancellationToken));

    [HttpGet("{workflowId:guid}")]
    public async Task<ActionResult<WorkflowDetailDto>> GetById(
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        var workflow = await _sender.Send(new GetWorkflowQuery(workflowId), cancellationToken);
        return workflow is null ? NotFound() : Ok(workflow);
    }

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

    [HttpPut("versions/{versionId:guid}/stages/{stageId:guid}")]
    public async Task<IActionResult> UpdateStage(Guid versionId, Guid stageId, UpdateWorkflowStageCommand command, CancellationToken cancellationToken)
    {
        if (versionId != command.WorkflowVersionId || stageId != command.StageId) return BadRequest("Route ids and command ids must match.");
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("versions/{versionId:guid}/stages/{stageId:guid}")]
    public async Task<IActionResult> DeleteStage(Guid versionId, Guid stageId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteWorkflowStageCommand(versionId, stageId), cancellationToken);
        return NoContent();
    }

    [HttpPut("versions/{versionId:guid}/transitions/{transitionId:guid}")]
    public async Task<IActionResult> UpdateTransition(Guid versionId, Guid transitionId, UpdateWorkflowTransitionCommand command, CancellationToken cancellationToken)
    {
        if (versionId != command.WorkflowVersionId || transitionId != command.TransitionId) return BadRequest("Route ids and command ids must match.");
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("versions/{versionId:guid}/transitions/{transitionId:guid}")]
    public async Task<IActionResult> DeleteTransition(Guid versionId, Guid transitionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteWorkflowTransitionCommand(versionId, transitionId), cancellationToken);
        return NoContent();
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
