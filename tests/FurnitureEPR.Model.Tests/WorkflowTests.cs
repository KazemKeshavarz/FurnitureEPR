using FurnitureEPR.Model.Workflow;
using Xunit;

namespace FurnitureEPR.Model.Tests;

public sealed class WorkflowTests
{
    [Fact]
    public void Published_version_cannot_be_modified()
    {
        var version = new WorkflowVersion(Guid.NewGuid(), 1);
        var stage = new WorkflowStage(
            version.Id,
            "مرحله اول",
            "stage-1",
            1);

        version.AddStage(stage);
        version.Publish();

        Assert.True(version.IsPublished);
        Assert.Throws<InvalidOperationException>(() =>
            version.AddStage(new WorkflowStage(
                version.Id,
                "مرحله دوم",
                "stage-2",
                2)));
        Assert.Throws<InvalidOperationException>(() =>
            version.AddTransition(new WorkflowTransition(
                version.Id,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "انتقال")));
    }

    [Fact]
    public void Workflow_stage_can_be_deactivated_and_reactivated()
    {
        var stage = new WorkflowStage(
            Guid.NewGuid(),
            "مرحله",
            "stage",
            1);

        stage.SetActive(false);
        Assert.False(stage.IsActive);

        stage.SetActive(true);
        Assert.True(stage.IsActive);
    }

    [Fact]
    public void Workflow_version_cannot_be_published_without_stages()
    {
        var version = new WorkflowVersion(Guid.NewGuid(), 1);

        Assert.Throws<InvalidOperationException>(() => version.Publish());
    }

    [Fact]
    public void Quality_control_rejection_keeps_current_stage_and_requires_new_approval()
    {
        var instance = new OrderWorkflowInstance(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        var stageId = instance.CurrentStageId;

        instance.RecordQualityControl(
            stageId,
            QualityControlResult.Rejected,
            "نیاز به اصلاح");

        Assert.Equal(stageId, instance.CurrentStageId);
        Assert.False(instance.IsQualityControlApproved(stageId));

        instance.RecordQualityControl(
            stageId,
            QualityControlResult.Approved,
            "اصلاح تأیید شد");

        Assert.Equal(stageId, instance.CurrentStageId);
        Assert.True(instance.IsQualityControlApproved(stageId));
        Assert.Equal(2, instance.QualityChecks.Count);
    }

    [Fact]
    public void Workflow_move_records_history_and_updates_current_stage()
    {
        var instance = new OrderWorkflowInstance(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        var fromStageId = instance.CurrentStageId;
        var toStageId = Guid.NewGuid();
        var transitionId = Guid.NewGuid();

        instance.MoveTo(toStageId, transitionId);

        Assert.Equal(toStageId, instance.CurrentStageId);
        var history = Assert.Single(instance.History);
        Assert.Equal(fromStageId, history.FromStageId);
        Assert.Equal(toStageId, history.ToStageId);
        Assert.Equal(transitionId, history.TransitionId);
    }

    [Fact]
    public void Completed_workflow_cannot_be_moved_or_completed_again()
    {
        var instance = new OrderWorkflowInstance(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        instance.Complete();

        Assert.Equal(OrderWorkflowInstanceStatus.Completed, instance.Status);
        Assert.NotNull(instance.CompletedAtUtc);
        Assert.Throws<InvalidOperationException>(() =>
            instance.MoveTo(Guid.NewGuid(), Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(() => instance.Complete());
    }

    [Fact]
    public void Workflow_stage_rejects_invalid_name_or_sort_order()
    {
        Assert.Throws<ArgumentException>(() =>
            new WorkflowStage(Guid.NewGuid(), " ", "stage", 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WorkflowStage(Guid.NewGuid(), "مرحله", "stage", -1));
    }
}
