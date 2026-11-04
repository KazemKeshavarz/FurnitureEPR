using Xunit;
using FurnitureEPR.Model.Orders;

namespace FurnitureEPR.Model.Tests;

public sealed class OrderTransferTests
{
    [Fact]
    public void Constructor_RejectsSameSourceAndTargetOrder()
    {
        var orderId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new OrderTransfer(orderId, orderId, Guid.NewGuid(), "اولویت تحویل"));
    }

    [Fact]
    public void AddLine_StoresPositiveTransferQuantity()
    {
        var transfer = new OrderTransfer(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "اولویت تحویل");

        transfer.AddLine(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2);

        Assert.Single(transfer.Lines);
        Assert.Equal(OrderTransferStatus.PendingApproval, transfer.Status);
    }

    [Fact]
    public void AddLine_RejectsNonPositiveQuantity()
    {
        var transfer = new OrderTransfer(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "اولویت تحویل");

        Assert.Throws<ArgumentException>(() =>
            transfer.AddLine(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0));
    }

    [Fact]
    public void Approve_ChangesStatusAndPreventsSecondReview()
    {
        var transfer = new OrderTransfer(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "اولویت تحویل");

        transfer.Approve(Guid.NewGuid());

        Assert.Equal(OrderTransferStatus.Approved, transfer.Status);
        Assert.NotNull(transfer.ReviewedAtUtc);
        Assert.Throws<InvalidOperationException>(() => transfer.Reject(Guid.NewGuid()));
    }

    [Fact]
    public void Reject_ChangesStatus()
    {
        var transfer = new OrderTransfer(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "اولویت تحویل");

        transfer.Reject(Guid.NewGuid());

        Assert.Equal(OrderTransferStatus.Rejected, transfer.Status);
        Assert.NotNull(transfer.ReviewedAtUtc);
    }
}
