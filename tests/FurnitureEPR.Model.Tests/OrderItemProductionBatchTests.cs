using FurnitureEPR.Model.Orders;
using Xunit;

namespace FurnitureEPR.Model.Tests;

public sealed class OrderItemProductionBatchTests
{
    [Fact]
    public void ReduceQuantity_KeepsRemainingReadyQuantity()
    {
        var batch = new OrderItemProductionBatch(Guid.NewGuid(), 6, ProductionBatchOrigin.Production, ProductionBatchStatus.Ready);

        batch.ReduceQuantity(2);

        Assert.Equal(4, batch.Quantity);
        Assert.Equal(ProductionBatchStatus.Ready, batch.Status);
    }

    [Fact]
    public void ReduceQuantity_CancelsBatchWhenNoQuantityRemains()
    {
        var batch = new OrderItemProductionBatch(Guid.NewGuid(), 2, ProductionBatchOrigin.Production, ProductionBatchStatus.Ready);

        batch.ReduceQuantity(2);

        Assert.Equal(0, batch.Quantity);
        Assert.Equal(ProductionBatchStatus.Cancelled, batch.Status);
    }

    [Fact]
    public void MarkReady_ChangesPlannedBatchToReady()
    {
        var batch = new OrderItemProductionBatch(Guid.NewGuid(), 3, ProductionBatchOrigin.Production);

        batch.MarkReady();

        Assert.Equal(ProductionBatchStatus.Ready, batch.Status);
        Assert.NotNull(batch.ReadyAtUtc);
    }

    [Fact]
    public void MarkInProgress_ChangesPlannedBatchToInProgress()
    {
        var batch = new OrderItemProductionBatch(Guid.NewGuid(), 3, ProductionBatchOrigin.Production);

        batch.MarkInProgress();

        Assert.Equal(ProductionBatchStatus.InProgress, batch.Status);
    }

    [Fact]
    public void TransferBatch_RequiresSourceTransferLine()
    {
        Assert.Throws<ArgumentException>(() =>
            new OrderItemProductionBatch(Guid.NewGuid(), 1, ProductionBatchOrigin.Transfer, ProductionBatchStatus.Ready));
    }
}
