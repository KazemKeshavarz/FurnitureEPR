using Xunit;
using FurnitureEPR.Model.Orders;

namespace FurnitureEPR.Model.Tests;

public sealed class OrderShortageTests
{
    [Fact]
    public void MarkHasShortage_ChangesCompletedOrderStatus()
    {
        var order = new Order(Guid.NewGuid());
        order.SetOrderNumber("T-100");
        order.AddItem(new OrderItem(Guid.NewGuid(), "صندلی", 2));
        order.FinalizeOrder();
        order.MarkCompleted();

        order.MarkHasShortage();

        Assert.Equal(OrderStatus.HasShortage, order.Status);
    }

    [Fact]
    public void MarkHasShortage_IsIdempotentForAlreadyShortOrder()
    {
        var order = new Order(Guid.NewGuid());
        order.SetOrderNumber("T-101");
        order.AddItem(new OrderItem(Guid.NewGuid(), "صندلی", 2));
        order.FinalizeOrder();
        order.MarkCompleted();
        order.MarkHasShortage();

        order.MarkHasShortage();

        Assert.Equal(OrderStatus.HasShortage, order.Status);
    }
    [Fact]
    public void CompletingRemainingWorkflows_DoesNotCompleteOrderWhileShortageExists()
    {
        var order = new Order(Guid.NewGuid());
        order.SetOrderNumber("T-102");
        order.AddItem(new OrderItem(Guid.NewGuid(), "صندلی", 2));
        order.FinalizeOrder();
        order.MarkHasShortage();

        order.MarkCompleted();

        Assert.Equal(OrderStatus.HasShortage, order.Status);
    }

}
