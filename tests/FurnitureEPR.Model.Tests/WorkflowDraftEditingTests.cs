using FurnitureEPR.Model.Workflow;
using Xunit;

namespace FurnitureEPR.Model.Tests;

public sealed class WorkflowDraftEditingTests
{
    [Fact]
    public void Draft_stage_can_be_updated_and_removed_after_its_transitions_are_deleted()
    {
        var version = new WorkflowVersion(Guid.NewGuid(), 1);
        var stage = new WorkflowStage(version.Id, "نجاری", "CARPENTRY", 1);
        version.AddStage(stage);

        version.UpdateStage(stage.Id, "نجاری اصلاح‌شده", "CARPENTRY_V2", 2, true, null);
        Assert.Equal("نجاری اصلاح‌شده", stage.Name);
        Assert.Equal(2, stage.SortOrder);
        Assert.True(stage.RequiresQualityControl);

        version.RemoveStage(stage.Id);
        Assert.Empty(version.Stages);
    }

    [Fact]
    public void Stage_cannot_be_removed_while_a_transition_references_it()
    {
        var version = new WorkflowVersion(Guid.NewGuid(), 1);
        var from = new WorkflowStage(version.Id, "نجاری", "CARPENTRY", 1);
        var to = new WorkflowStage(version.Id, "رویه‌کوبی", "UPHOLSTERY", 2);
        version.AddStage(from);
        version.AddStage(to);
        version.AddTransition(new WorkflowTransition(version.Id, from.Id, to.Id, "ادامه"));

        Assert.Throws<InvalidOperationException>(() => version.RemoveStage(from.Id));
    }

    [Fact]
    public void Draft_transition_can_be_updated_and_deleted_but_published_version_is_immutable()
    {
        var version = new WorkflowVersion(Guid.NewGuid(), 1);
        var first = new WorkflowStage(version.Id, "نجاری", "CARPENTRY", 1);
        var second = new WorkflowStage(version.Id, "رویه‌کوبی", "UPHOLSTERY", 2);
        var third = new WorkflowStage(version.Id, "بسته‌بندی", "PACKING", 3);
        version.AddStage(first); version.AddStage(second); version.AddStage(third);
        var transition = new WorkflowTransition(version.Id, first.Id, second.Id, "مسیر قبلی");
        version.AddTransition(transition);

        version.UpdateTransition(transition.Id, first.Id, third.Id, "مسیر اصلاح‌شده");
        Assert.Equal(third.Id, transition.ToStageId);
        Assert.Equal("مسیر اصلاح‌شده", transition.Name);
        version.RemoveTransition(transition.Id);
        Assert.Empty(version.Transitions);

        version.Publish();
        Assert.Throws<InvalidOperationException>(() => version.RemoveTransition(transition.Id));
    }
}
