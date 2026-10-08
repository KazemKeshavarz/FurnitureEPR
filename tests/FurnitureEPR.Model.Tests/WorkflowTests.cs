using FurnitureEPR.Model.Workflow;

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
}
