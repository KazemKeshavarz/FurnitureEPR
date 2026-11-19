using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureEPR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class secondInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderWorkflowInstances_OrderId_CategoryId",
                table: "OrderWorkflowInstances");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductionBatchId",
                table: "OrderWorkflowInstances",
                type: "uniqueidentifier",
                nullable: true);

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
                    table.ForeignKey(
                        name: "FK_OrderTransfers_Orders_SourceOrderId",
                        column: x => x.SourceOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderTransfers_Orders_TargetOrderId",
                        column: x => x.TargetOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
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
                    table.ForeignKey(
                        name: "FK_OrderItemProductionBatches_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemProductionBatches_Warehouses_ReplenishmentWarehouseId",
                        column: x => x.ReplenishmentWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_OrderItemWarehouseAllocations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemWarehouseAllocations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_OrderTransferLines_OrderItems_SourceOrderItemId",
                        column: x => x.SourceOrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderTransferLines_OrderItems_TargetOrderItemId",
                        column: x => x.TargetOrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderTransferLines_OrderTransfers_OrderTransferId",
                        column: x => x.OrderTransferId,
                        principalTable: "OrderTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTransferLines_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemProductionBatches_OrderItemId_Status",
                table: "OrderItemProductionBatches",
                columns: new[] { "OrderItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemProductionBatches_ReplenishmentWarehouseId",
                table: "OrderItemProductionBatches",
                column: "ReplenishmentWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemProductionBatches_SourceTransferLineId",
                table: "OrderItemProductionBatches",
                column: "SourceTransferLineId",
                unique: true,
                filter: "[SourceTransferLineId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemWarehouseAllocations_OrderItemId_WarehouseId",
                table: "OrderItemWarehouseAllocations",
                columns: new[] { "OrderItemId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemWarehouseAllocations_WarehouseId",
                table: "OrderItemWarehouseAllocations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransferLines_OrderTransferId",
                table: "OrderTransferLines",
                column: "OrderTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransferLines_SourceOrderItemId",
                table: "OrderTransferLines",
                column: "SourceOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransferLines_TargetOrderItemId",
                table: "OrderTransferLines",
                column: "TargetOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransferLines_WarehouseId",
                table: "OrderTransferLines",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransfers_SourceOrderId_RequestedAtUtc",
                table: "OrderTransfers",
                columns: new[] { "SourceOrderId", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransfers_TargetOrderId_RequestedAtUtc",
                table: "OrderTransfers",
                columns: new[] { "TargetOrderId", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemProductionBatches");

            migrationBuilder.DropTable(
                name: "OrderItemWarehouseAllocations");

            migrationBuilder.DropTable(
                name: "OrderTransferLines");

            migrationBuilder.DropTable(
                name: "OrderTransfers");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_OrderWorkflowInstances_OrderId_CategoryId",
                table: "OrderWorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_OrderWorkflowInstances_ProductionBatchId",
                table: "OrderWorkflowInstances");

            migrationBuilder.DropColumn(
                name: "ProductionBatchId",
                table: "OrderWorkflowInstances");

            migrationBuilder.CreateIndex(
                name: "IX_OrderWorkflowInstances_OrderId_CategoryId",
                table: "OrderWorkflowInstances",
                columns: new[] { "OrderId", "CategoryId" },
                unique: true);
        }
    }
}
