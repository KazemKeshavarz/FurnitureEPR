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
    public void Quality_control_must_be_approved_before_moving_forward()
    {
        var instance = new OrderWorkflowInstance(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var stageId = instance.CurrentStageId;

        Assert.False(instance.IsQualityControlApproved(stageId));

        instance.RecordQualityControl(
            stageId,
            QualityControlResult.Rejected,
            "نیاز به اصلاح");

        Assert.False(instance.IsQualityControlApproved(stageId));

        instance.RecordQualityControl(
            stageId,
            QualityControlResult.Approved,
            "تأیید شد",
            Guid.NewGuid());

        Assert.True(instance.IsQualityControlApproved(stageId));
    }
}
