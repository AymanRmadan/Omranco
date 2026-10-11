using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omranco.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CopmletePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymentId",
                table: "PaymentAttempts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymobIntentionId",
                table: "PaymentAttempts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymobOrderId",
                table: "PaymentAttempts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymobTransactionId",
                table: "PaymentAttempts");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "PaymentAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentRefunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    PaymentAttemptId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PaymobRefundTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FailureCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailureMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_PaymentAttempts_PaymentAttemptId",
                        column: x => x.PaymentAttemptId,
                        principalTable: "PaymentAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderCancellations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymentRefundId = table.Column<int>(type: "int", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderCancellations_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderCancellations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderCancellations_PaymentRefunds_PaymentRefundId",
                        column: x => x.PaymentRefundId,
                        principalTable: "PaymentRefunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDisputes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OpenedByAccountUserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovedRefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PaymentRefundId = table.Column<int>(type: "int", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDisputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDisputes_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDisputes_CustomerAccountUsers_OpenedByAccountUserId",
                        column: x => x.OpenedByAccountUserId,
                        principalTable: "CustomerAccountUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDisputes_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDisputes_PaymentRefunds_PaymentRefundId",
                        column: x => x.PaymentRefundId,
                        principalTable: "PaymentRefunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDisputeAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDisputeId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDisputeAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDisputeAttachments_OrderDisputes_OrderDisputeId",
                        column: x => x.OrderDisputeId,
                        principalTable: "OrderDisputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymentId_Status_ExpiresAt",
                table: "PaymentAttempts",
                columns: new[] { "PaymentId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymobIntentionId",
                table: "PaymentAttempts",
                column: "PaymobIntentionId",
                unique: true,
                filter: "[PaymobIntentionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymobOrderId",
                table: "PaymentAttempts",
                column: "PaymobOrderId",
                unique: true,
                filter: "[PaymobOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymobTransactionId",
                table: "PaymentAttempts",
                column: "PaymobTransactionId",
                unique: true,
                filter: "[PaymobTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCancellations_OrderId",
                table: "OrderCancellations",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderCancellations_PaymentRefundId",
                table: "OrderCancellations",
                column: "PaymentRefundId",
                unique: true,
                filter: "[PaymentRefundId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCancellations_RequestedByUserId",
                table: "OrderCancellations",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDisputeAttachments_OrderDisputeId",
                table: "OrderDisputeAttachments",
                column: "OrderDisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDisputes_OpenedByAccountUserId",
                table: "OrderDisputes",
                column: "OpenedByAccountUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDisputes_OrderId_Status",
                table: "OrderDisputes",
                columns: new[] { "OrderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDisputes_PaymentRefundId",
                table: "OrderDisputes",
                column: "PaymentRefundId",
                unique: true,
                filter: "[PaymentRefundId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDisputes_ReviewedByUserId",
                table: "OrderDisputes",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_PaymentAttemptId",
                table: "PaymentRefunds",
                column: "PaymentAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_PaymentId_Status",
                table: "PaymentRefunds",
                columns: new[] { "PaymentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_PaymobRefundTransactionId",
                table: "PaymentRefunds",
                column: "PaymobRefundTransactionId",
                unique: true,
                filter: "[PaymobRefundTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_Reference",
                table: "PaymentRefunds",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_RequestedByUserId",
                table: "PaymentRefunds",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderCancellations");

            migrationBuilder.DropTable(
                name: "OrderDisputeAttachments");

            migrationBuilder.DropTable(
                name: "OrderDisputes");

            migrationBuilder.DropTable(
                name: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymentId_Status_ExpiresAt",
                table: "PaymentAttempts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymobIntentionId",
                table: "PaymentAttempts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymobOrderId",
                table: "PaymentAttempts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_PaymobTransactionId",
                table: "PaymentAttempts");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "PaymentAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymentId",
                table: "PaymentAttempts",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymobIntentionId",
                table: "PaymentAttempts",
                column: "PaymobIntentionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymobOrderId",
                table: "PaymentAttempts",
                column: "PaymobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymobTransactionId",
                table: "PaymentAttempts",
                column: "PaymobTransactionId");
        }
    }
}
