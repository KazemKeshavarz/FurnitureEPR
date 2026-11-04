using System;
using FurnitureEPR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureEPR.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261104193000_AddWarehouseTransferAndProductionBatches")]
public partial class AddWarehouseTransferAndProductionBatches : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ProductionBatchId",
            table: "OrderWorkflowInstances",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.DropIndex(
            name: "IX_OrderWorkflowInstances_OrderId_CategoryId",
            table: "OrderWorkflowInstances");

        migrationBuilder.CreateIndex(
            name: "IX_OrderWorkflowInstances_OrderId_CategoryId",
            table: "OrderWorkflowInstances",
            columns: new[] { "OrderId", "CategoryId" });

        migrationBuilder.CreateIndex(
            name: "IX_OrderWorkflowInstances_ProductionBatchId",
            table: "OrderWorkflowInstances",
            column: "ProductionBatchId",
            unique: true,
            filter: "[ProductionBatchId] IS NOT NULL");

        migrationBuilder.CreateTable(
            name: "Warehouses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Warehouses", x => x.Id));

        migrationBuilder.CreateTable(
            name: "OrderTransfers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SourceOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TargetOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderTransfers", x => x.Id);
                table.ForeignKey("FK_OrderTransfers_Orders_SourceOrderId", x => x.SourceOrderId, "Orders", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_OrderTransfers_Orders_TargetOrderId", x => x.TargetOrderId, "Orders", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OrderItemProductionBatches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                Origin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                SourceTransferLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ReplenishmentWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReadyAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItemProductionBatches", x => x.Id);
                table.ForeignKey("FK_OrderItemProductionBatches_OrderItems_OrderItemId", x => x.OrderItemId, "OrderItems", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_OrderItemProductionBatches_Warehouses_ReplenishmentWarehouseId", x => x.ReplenishmentWarehouseId, "Warehouses", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OrderItemWarehouseAllocations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItemWarehouseAllocations", x => x.Id);
                table.ForeignKey("FK_OrderItemWarehouseAllocations_OrderItems_OrderItemId", x => x.OrderItemId, "OrderItems", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_OrderItemWarehouseAllocations_Warehouses_WarehouseId", x => x.WarehouseId, "Warehouses", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OrderTransferLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderTransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SourceOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TargetOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderTransferLines", x => x.Id);
                table.ForeignKey("FK_OrderTransferLines_OrderTransfers_OrderTransferId", x => x.OrderTransferId, "OrderTransfers", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_OrderTransferLines_OrderItems_SourceOrderItemId", x => x.SourceOrderItemId, "OrderItems", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_OrderTransferLines_OrderItems_TargetOrderItemId", x => x.TargetOrderItemId, "OrderItems", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_OrderTransferLines_Warehouses_WarehouseId", x => x.WarehouseId, "Warehouses", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_Warehouses_Code", "Warehouses", "Code", unique: true);
        migrationBuilder.CreateIndex("IX_OrderTransfers_SourceOrderId_RequestedAtUtc", "OrderTransfers", new[] { "SourceOrderId", "RequestedAtUtc" });
        migrationBuilder.CreateIndex("IX_OrderTransfers_TargetOrderId_RequestedAtUtc", "OrderTransfers", new[] { "TargetOrderId", "RequestedAtUtc" });
        migrationBuilder.CreateIndex("IX_OrderItemProductionBatches_OrderItemId_Status", "OrderItemProductionBatches", new[] { "OrderItemId", "Status" });
        migrationBuilder.CreateIndex("IX_OrderItemProductionBatches_SourceTransferLineId", "OrderItemProductionBatches", "SourceTransferLineId", unique: true, filter: "[SourceTransferLineId] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_OrderItemProductionBatches_ReplenishmentWarehouseId", "OrderItemProductionBatches", "ReplenishmentWarehouseId");
        migrationBuilder.CreateIndex("IX_OrderItemWarehouseAllocations_OrderItemId_WarehouseId", "OrderItemWarehouseAllocations", new[] { "OrderItemId", "WarehouseId" }, unique: true);
        migrationBuilder.CreateIndex("IX_OrderItemWarehouseAllocations_WarehouseId", "OrderItemWarehouseAllocations", "WarehouseId");
        migrationBuilder.CreateIndex("IX_OrderTransferLines_OrderTransferId", "OrderTransferLines", "OrderTransferId");
        migrationBuilder.CreateIndex("IX_OrderTransferLines_SourceOrderItemId", "OrderTransferLines", "SourceOrderItemId");
        migrationBuilder.CreateIndex("IX_OrderTransferLines_TargetOrderItemId", "OrderTransferLines", "TargetOrderItemId");
        migrationBuilder.CreateIndex("IX_OrderTransferLines_WarehouseId", "OrderTransferLines", "WarehouseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("OrderTransferLines");
        migrationBuilder.DropTable("OrderItemWarehouseAllocations");
        migrationBuilder.DropTable("OrderItemProductionBatches");
        migrationBuilder.DropTable("OrderTransfers");
        migrationBuilder.DropTable("Warehouses");
        migrationBuilder.DropIndex("IX_OrderWorkflowInstances_ProductionBatchId", "OrderWorkflowInstances");
        migrationBuilder.DropIndex("IX_OrderWorkflowInstances_OrderId_CategoryId", "OrderWorkflowInstances");
        migrationBuilder.CreateIndex(
            name: "IX_OrderWorkflowInstances_OrderId_CategoryId",
            table: "OrderWorkflowInstances",
            columns: new[] { "OrderId", "CategoryId" },
            unique: true);
        migrationBuilder.DropColumn("ProductionBatchId", "OrderWorkflowInstances");
    }
}
